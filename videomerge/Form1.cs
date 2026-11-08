using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace videomerge
{
    public partial class Form1 : Form
    {
        private bool isMerging;

        public Form1()
        {
            InitializeComponent();
        }

        private sealed class VideoInfo
        {
            public string Path { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public double SampleAspectRatio { get; set; } = 1.0;
            public double Rotation { get; set; }

            public int DisplayWidth =>
                Math.Max(2, (int)Math.Round(
                    Width * SampleAspectRatio,
                    MidpointRounding.AwayFromZero));

            public int DisplayHeight => Height;

            public bool IsRotated =>
                Math.Abs(Rotation % 180) == 90;
        }

        private static string FindExecutable(string name)
        {
            string[] extensions = { ".exe", "" };

            foreach (string directory in
                (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(directory))
                    continue;

                foreach (string extension in extensions)
                {
                    string candidate = Path.Combine(
                        directory.Trim('"'),
                        name + extension);

                    if (File.Exists(candidate))
                        return candidate;
                }
            }

            return name;
        }

        private static string QuoteArgument(string argument)
        {
            if (argument == null)
                return "\"\"";

            // Windows command-line quoting:
            // preserve backslashes except when they precede quotes
            // or the closing quote.
            var result = new StringBuilder();
            result.Append('"');

            int backslashes = 0;

            foreach (char c in argument)
            {
                if (c == '\\')
                {
                    backslashes++;
                }
                else if (c == '"')
                {
                    result.Append('\\', backslashes * 2 + 1);
                    result.Append('"');
                    backslashes = 0;
                }
                else
                {
                    if (backslashes > 0)
                    {
                        result.Append('\\', backslashes);
                        backslashes = 0;
                    }

                    result.Append(c);
                }
            }

            if (backslashes > 0)
                result.Append('\\', backslashes * 2);

            result.Append('"');
            return result.ToString();
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(message)));
                return;
            }

            txtLog.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}" +
                Environment.NewLine);
        }

        private static async Task<(int ExitCode, string Output, string Error)>
            RunProcessAsync(string executable, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = psi })
            {
                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                        stdout.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                        stderr.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                return (
                    process.ExitCode,
                    stdout.ToString(),
                    stderr.ToString()
                );
            }
        }

        private static double ParseAspectRatio(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return 1.0;

            string[] parts = value.Split(':');

            if (parts.Length == 2 &&
                double.TryParse(parts[0], NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double numerator) &&
                double.TryParse(parts[1], NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double denominator) &&
                denominator != 0)
            {
                double ratio = numerator / denominator;
                return ratio > 0 &&
       !double.IsNaN(ratio) &&
       !double.IsInfinity(ratio)
    ? ratio
    : 1.0;
            }

            return 1.0;
        }

        private async Task<VideoInfo> GetVideoInfoAsync(string path)
        {
            string ffprobe = FindExecutable("ffprobe");

            string args =
                "-v error -select_streams v:0 " +
                "-show_entries stream=width,height,sample_aspect_ratio:stream_tags=rotate:stream_side_data=rotation " +
                "-of default=noprint_wrappers=1 " +
                QuoteArgument(path);

            var result = await RunProcessAsync(ffprobe, args);

            if (result.ExitCode != 0)
                throw new Exception(
                    $"ffprobe failed for {Path.GetFileName(path)}:\n{result.Error}");

            var info = new VideoInfo { Path = path };

            foreach (string line in result.Output.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(new[] { '=' }, 2);

                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (key == "width" &&
                    int.TryParse(value, out int width))
                    info.Width = width;

                else if (key == "height" &&
                    int.TryParse(value, out int height))
                    info.Height = height;

                else if (key == "sample_aspect_ratio")
                    info.SampleAspectRatio = ParseAspectRatio(value);

                else if ((key == "rotation" || key == "TAG:rotate") &&
                    double.TryParse(value, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out double rotation))
                    info.Rotation = rotation;
            }

            if (info.Width <= 0 || info.Height <= 0)
                throw new Exception(
                    $"Could not determine video dimensions for {Path.GetFileName(path)}.");

            return info;
        }

        private async Task<bool> IsNvencAvailableAsync()
        {
            var result = await RunProcessAsync(
                FindExecutable("ffmpeg"),
                "-hide_banner -encoders");

            return result.ExitCode == 0 &&
                result.Output.IndexOf(
                    "h264_nvenc",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string EscapeConcatPath(string path)
        {
            return path.Replace("\\", "/").Replace("'", "'\\''");
        }

        private static string GetErrorTail(string text, int maxLines = 15)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "(no error details)";

            string[] lines = text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            return string.Join(
                Environment.NewLine,
                lines.Skip(Math.Max(0, lines.Length - maxLines)));
        }

        private void SetMergingState(bool merging)
        {
            isMerging = merging;

            btnAddVideos.Enabled = !merging;
            btnMoveUp.Enabled = !merging;
            btnMoveDown.Enabled = !merging;
            btnRemove.Enabled = !merging;
            btnClear.Enabled = !merging;
            btnMerge.Enabled = !merging;

            lblStatus.Text = merging ? "Merging..." : "Ready";
            progressBar.Value = merging ? 0 : 100;
        }

        private void btnAddVideos_Click(object sender, EventArgs e)
        {
            if (openFileDialogVideos.ShowDialog() != DialogResult.OK)
                return;

            foreach (string file in openFileDialogVideos.FileNames)
            {
                if (!lstVideos.Items.Cast<object>()
                    .Any(item => string.Equals(
                        item.ToString(), file,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    lstVideos.Items.Add(file);
                }
            }

            lblStatus.Text = $"{lstVideos.Items.Count} video(s)";
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            int index = lstVideos.SelectedIndex;
            if (index <= 0)
                return;

            object item = lstVideos.Items[index];
            lstVideos.Items.RemoveAt(index);
            lstVideos.Items.Insert(index - 1, item);
            lstVideos.SelectedIndex = index - 1;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            int index = lstVideos.SelectedIndex;
            if (index < 0 || index >= lstVideos.Items.Count - 1)
                return;

            object item = lstVideos.Items[index];
            lstVideos.Items.RemoveAt(index);
            lstVideos.Items.Insert(index + 1, item);
            lstVideos.SelectedIndex = index + 1;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int index = lstVideos.SelectedIndex;
            if (index >= 0)
                lstVideos.Items.RemoveAt(index);

            lblStatus.Text = $"{lstVideos.Items.Count} video(s)";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstVideos.Items.Clear();
            lblStatus.Text = "Ready";
        }

        private async void btnMerge_Click(object sender, EventArgs e)
        {
            if (isMerging)
                return;

            if (lstVideos.Items.Count == 0)
            {
                MessageBox.Show("Please add videos first.");
                return;
            }

            saveFileDialogOutput.Title = "Save Merged Video";
            saveFileDialogOutput.Filter =
                "MTS Video (*.mts)|*.mts|" +
                "MP4 Video (*.mp4)|*.mp4|" +
                "MKV Video (*.mkv)|*.mkv|" +
                "All Files (*.*)|*.*";
            saveFileDialogOutput.DefaultExt = "mts";
            saveFileDialogOutput.AddExtension = true;
            saveFileDialogOutput.FilterIndex = 1;

            if (saveFileDialogOutput.ShowDialog() != DialogResult.OK)
                return;

            string outputPath = saveFileDialogOutput.FileName;
            string outputDirectory = Path.GetDirectoryName(outputPath);

            string[] inputFiles = lstVideos.Items
                .Cast<object>()
                .Select(item => item.ToString())
                .ToArray();

            string tempDirectory = Path.Combine(
                Path.GetTempPath(),
                "videomerge_" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(tempDirectory);

            SetMergingState(true);
            txtLog.Clear();

            try
            {
                Log("Reading video dimensions and display aspect ratios...");

                var videoInfos = new List<VideoInfo>();

                foreach (string file in inputFiles)
                {
                    VideoInfo info = await GetVideoInfoAsync(file);
                    videoInfos.Add(info);

                    Log($"{Path.GetFileName(file)}: " +
                        $"{info.Width}x{info.Height}, " +
                        $"SAR={info.SampleAspectRatio.ToString("0.####", CultureInfo.InvariantCulture)}:1, " +
                        $"display={info.DisplayWidth}x{info.DisplayHeight}");
                }

                // Apply rotation to the display dimensions when selecting
                // the common canvas.
                var displaySizes = videoInfos.Select(v =>
                {
                    int width = v.DisplayWidth;
                    int height = v.DisplayHeight;

                    return v.IsRotated
                        ? (Width: height, Height: width)
                        : (Width: width, Height: height);
                }).ToList();

                int targetWidth = displaySizes.Min(s => s.Width);
                int targetHeight = displaySizes.Min(s => s.Height);

                targetWidth -= targetWidth % 2;
                targetHeight -= targetHeight % 2;

                Log($"Common output canvas: {targetWidth}x{targetHeight}");

                bool useNvenc = await IsNvencAvailableAsync();
                Log(useNvenc
                    ? "NVIDIA H.264 NVENC encoder detected."
                    : "NVENC not detected; using CPU encoding.");

                string extension = Path.GetExtension(outputPath)
                    .ToLowerInvariant();

                bool isMp4Family = extension == ".mp4" ||
                    extension == ".m4v" ||
                    extension == ".mov";

                bool isMkv = extension == ".mkv";
                bool isMtsFamily = extension == ".mts" ||
                    extension == ".m2ts";

                string videoCodec;
                string audioCodec;
                string containerArgs = "";

                if (isMp4Family || isMtsFamily || isMkv)
                {
                    videoCodec = useNvenc
                        ? "-c:v h264_nvenc -preset p5 -rc vbr -cq 20 -b:v 0"
                        : "-c:v libx264 -preset medium -crf 20";

                    audioCodec = "-c:a aac -b:a 192k";

                    if (isMp4Family)
                        containerArgs = "-movflags +faststart";
                }
                else if (extension == ".avi")
                {
                    videoCodec = "-c:v mpeg4 -q:v 3";
                    audioCodec = "-c:a libmp3lame -b:a 192k";
                }
                else if (extension == ".webm")
                {
                    videoCodec = "-c:v libvpx-vp9 -crf 30 -b:v 0";
                    audioCodec = "-c:a libopus -b:a 128k";
                }
                else
                {
                    videoCodec = useNvenc
                        ? "-c:v h264_nvenc -preset p5 -rc vbr -cq 20 -b:v 0"
                        : "-c:v libx264 -preset medium -crf 20";

                    audioCodec = "-c:a aac -b:a 192k";
                }

                string ffmpeg = FindExecutable("ffmpeg");
                var normalizedFiles = new List<string>();

                for (int i = 0; i < videoInfos.Count; i++)
                {
                    VideoInfo info = videoInfos[i];

                    string normalizedPath = Path.Combine(
                        tempDirectory, $"clip_{i:D5}.mkv");

                    normalizedFiles.Add(normalizedPath);

                    // Correct the input's non-square pixel geometry first.
                    // Then scale it to fit the shared display canvas and pad
                    // without stretching or changing the intended DAR.
                    string filter =
                        $"scale=w=trunc(iw*sar/2)*2:h=ih:flags=lanczos," +
                        $"setsar=1," +
                        $"scale=w={targetWidth}:h={targetHeight}:" +
                        $"force_original_aspect_ratio=decrease:flags=lanczos," +
                        $"pad={targetWidth}:{targetHeight}:(ow-iw)/2:(oh-ih)/2," +
                        $"setsar=1,fps=30,format=yuv420p";

                    string args =
                        "-hide_banner -y " +
                        "-i " + QuoteArgument(info.Path) + " " +
                        "-map 0:v:0 -map 0:a:0? " +
                        "-vf " + QuoteArgument(filter) + " " +
                        "-af " + QuoteArgument(
                            "aresample=48000, aformat=sample_fmts=fltp:channel_layouts=stereo") + " " +
                        videoCodec + " " +
                        audioCodec + " " +
                        "-max_muxing_queue_size 2048 " +
                        QuoteArgument(normalizedPath);

                    Log($"Normalizing {i + 1}/{videoInfos.Count}: " +
                        Path.GetFileName(info.Path));

                    var result = await RunProcessAsync(ffmpeg, args);

                    if (result.ExitCode != 0)
                    {
                        throw new Exception(
                            $"Failed to process {Path.GetFileName(info.Path)}:\n" +
                            GetErrorTail(result.Error));
                    }

                    progressBar.Value = Math.Min(
                        90,
                        (int)Math.Round(
                            90.0 * (i + 1) / videoInfos.Count));
                }

                string concatFile = Path.Combine(
                    tempDirectory, "concat.txt");

                File.WriteAllLines(
                    concatFile,
                    normalizedFiles.Select(
                        f => "file '" + EscapeConcatPath(f) + "'"),
                    new UTF8Encoding(false));

                Log("Joining normalized clips...");

                // All clips have already been encoded with identical
                // dimensions, pixel format, frame rate and codecs.
                string concatArgs =
                    "-hide_banner -y " +
                    "-f concat -safe 0 " +
                    "-i " + QuoteArgument(concatFile) + " " +
                    "-c copy " +
                    containerArgs + " " +
                    QuoteArgument(outputPath);

                var concatResult = await RunProcessAsync(
                    ffmpeg, concatArgs);

                if (concatResult.ExitCode != 0)
                {
                    throw new Exception(
                        "Failed to join video clips:\n" +
                        GetErrorTail(concatResult.Error));
                }

                progressBar.Value = 100;
                lblStatus.Text = "Completed";

                Log("Merge completed successfully.");
                Log($"Output: {outputPath}");

                MessageBox.Show(
                    "Video merge completed successfully.",
                    "Completed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
                lblStatus.Text = "Failed";

                MessageBox.Show(
                    ex.Message,
                    "Merge failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetMergingState(false);

                try
                {
                    if (Directory.Exists(tempDirectory))
                        Directory.Delete(tempDirectory, true);
                }
                catch
                {
                    Log("Warning: temporary files could not be fully deleted.");
                }
            }
        }
    }
}
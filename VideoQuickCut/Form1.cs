using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoQuickCut
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeIntervals();
        }

        private class VideoInterval
        {
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }
        }

        private readonly List<Tuple<MaskedTextBox, MaskedTextBox>>
            intervalInputs =
                new List<Tuple<MaskedTextBox, MaskedTextBox>>();

        private void InitializeIntervals()
        {
            flowLayoutPanelIntervals.FlowDirection =
                FlowDirection.TopDown;

            flowLayoutPanelIntervals.WrapContents = false;
            flowLayoutPanelIntervals.AutoScroll = true;

            AddIntervalRow();

            // Wire this only once. If you already connected the
            // Click event in the Designer, remove this line.
            btnAddInterval.Click += btnAddInterval_Click;
        }


        private async Task<bool> IsNvencAvailableAsync()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "ffmpeg";
                psi.Arguments = "-hide_banner -h encoder=h264_nvenc";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.Start();

                    Task<string> stdout =
                        process.StandardOutput.ReadToEndAsync();
                    Task<string> stderr =
                        process.StandardError.ReadToEndAsync();

                    await Task.Run(() => process.WaitForExit());

                    string output = (await stdout) + (await stderr);

                    return process.ExitCode == 0 &&
                           output.IndexOf(
                               "h264_nvenc",
                               StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }


        private void btnAddInterval_Click(object sender, EventArgs e)
        {
            AddIntervalRow();
        }

        private void AddIntervalRow()
        {
            int number = intervalInputs.Count + 1;

            FlowLayoutPanel row = new FlowLayoutPanel();
            row.FlowDirection = FlowDirection.LeftToRight;
            row.WrapContents = false;
            row.AutoSize = true;
            row.Margin = new Padding(3);
            row.Padding = new Padding(2);

            Label intervalLabel = new Label();
            intervalLabel.Text = "Interval " + number;
            intervalLabel.Width = 75;
            intervalLabel.Height = 28;
            intervalLabel.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            MaskedTextBox startBox = new MaskedTextBox();
            startBox.Mask = "00:00:00";
            startBox.TextMaskFormat = MaskFormat.IncludeLiterals;
            startBox.Text = "00:00:00";
            startBox.Width = 80;
            startBox.PromptChar = '_';

            Label toLabel = new Label();
            toLabel.Text = "to";
            toLabel.Width = 25;
            toLabel.Height = 28;
            toLabel.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            MaskedTextBox endBox = new MaskedTextBox();
            endBox.Mask = "00:00:00";
            endBox.TextMaskFormat = MaskFormat.IncludeLiterals;
            endBox.Text = "00:00:00";
            endBox.Width = 80;
            endBox.PromptChar = '_';

            row.Controls.Add(intervalLabel);
            row.Controls.Add(startBox);
            row.Controls.Add(toLabel);
            row.Controls.Add(endBox);

            flowLayoutPanelIntervals.Controls.Add(row);

            intervalInputs.Add(
                Tuple.Create(startBox, endBox));
        }

        private List<VideoInterval> GetIntervals(
            double videoDuration)
        {
            List<VideoInterval> intervals =
                new List<VideoInterval>();

            TimeSpan previousEnd = TimeSpan.Zero;

            for (int i = 0; i < intervalInputs.Count; i++)
            {
                MaskedTextBox startBox =
                    intervalInputs[i].Item1;

                MaskedTextBox endBox =
                    intervalInputs[i].Item2;

                TimeSpan start;
                TimeSpan end;

                if (!startBox.MaskCompleted ||
                    !endBox.MaskCompleted ||
                    !TimeSpan.TryParseExact(
                        startBox.Text,
                        @"hh\:mm\:ss",
                        CultureInfo.InvariantCulture,
                        out start) ||
                    !TimeSpan.TryParseExact(
                        endBox.Text,
                        @"hh\:mm\:ss",
                        CultureInfo.InvariantCulture,
                        out end))
                {
                    throw new Exception(
                        "Enter valid HH:mm:ss values for interval " +
                        (i + 1) + ".");
                }

                if (end <= start)
                {
                    throw new Exception(
                        "The end time must be later than the start time " +
                        "for interval " + (i + 1) + ".");
                }

                if (end.TotalSeconds > videoDuration)
                {
                    throw new Exception(
                        "Interval " + (i + 1) +
                        " exceeds the video duration.");
                }

                if (i > 0 && start < previousEnd)
                {
                    throw new Exception(
                        "Intervals must be in chronological order " +
                        "and must not overlap.");
                }

                intervals.Add(new VideoInterval
                {
                    Start = start,
                    End = end
                });

                previousEnd = end;
            }

            return intervals;
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description =
        "Select the Output Folder";
            folderBrowserDialog1.ShowNewFolderButton = true;

            if (Directory.Exists(txtOutputVideo.Text))
            {
                folderBrowserDialog1.SelectedPath = txtOutputVideo.Text;
            }

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtOutputVideo.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnBrowseVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select Input Video";
            openFileDialog1.Filter =
                "Video Files|*.mp4;*.mts;*.mkv;*.mov;*.avi;*.webm;*.m2ts|" +
                "All Files|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtInputVideo.Text = openFileDialog1.FileName;

                // Set the output folder to the input video's directory.
                string inputFolder =
                    Path.GetDirectoryName(openFileDialog1.FileName);

                if (!string.IsNullOrEmpty(inputFolder))
                {
                    txtOutputVideo.Text = inputFolder;
                }
            }
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            openFileDialog2.Title = "Select Replacement Image";
            openFileDialog2.Filter =
                "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.webp|" +
                "All Files|*.*";

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                txtReplacementImage.Text = openFileDialog2.FileName;
            }
        }

        private void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke((Action)(() => LogMessage(message)));
                return;
            }

            txtLog.AppendText(
                "[" + DateTime.Now.ToString("HH:mm:ss") + "] " +
                message + Environment.NewLine);
        }

        private void UpdateProgress(int percent)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.BeginInvoke(
                    (Action)(() => UpdateProgress(percent)));
                return;
            }

            percent = Math.Max(0, Math.Min(100, percent));
            progressBar1.Value = percent;
        }

        private async void btnProcess_Click(object sender, EventArgs e)
        {
            string inputVideo = txtInputVideo.Text.Trim();
            string imagePath = txtReplacementImage.Text.Trim();
            string outputFolder = txtOutputVideo.Text.Trim();

            if (!File.Exists(inputVideo))
            {
                MessageBox.Show("Please select a valid input video.");
                return;
            }

            if (!File.Exists(imagePath))
            {
                MessageBox.Show("Please select a valid replacement image.");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                MessageBox.Show("Please select a valid output folder.");
                return;
            }

            string extension = Path.GetExtension(inputVideo);


            string cpuVideoCodec;
            string gpuVideoCodec = null;
            string audioCodec;
            string audioBitrate = "192k";

            switch (extension.ToLowerInvariant())
            {
                case ".mts":
                case ".m2ts":
                case ".mp4":
                case ".m4v":
                case ".mov":
                case ".mkv":
                    cpuVideoCodec = "libx264";
                    gpuVideoCodec = "h264_nvenc";
                    audioCodec = "aac";
                    break;

                case ".avi":
                    cpuVideoCodec = "mpeg4";
                    audioCodec = "libmp3lame";
                    break;

                case ".webm":
                    cpuVideoCodec = "libvpx-vp9";
                    audioCodec = "libopus";
                    audioBitrate = "128k";
                    break;

                default:
                    MessageBox.Show(
                        "Unsupported output format: " + extension);
                    return;
            }

            bool useGpu = gpuVideoCodec != null &&
              await IsNvencAvailableAsync();

            if (!useGpu)
            {
                DialogResult result = MessageBox.Show(
                    "NVIDIA GPU encoding (NVENC) was not detected or is " +
                    "unavailable for this video format.\r\n\r\n" +
                    "Do you want to continue using CPU encoding instead?",
                    "GPU Not Available",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Cancel)
                {
                    LogMessage("Processing cancelled by user.");
                    return;
                }

                LogMessage("User confirmed CPU encoding.");
            }
            else
            {
                LogMessage("NVIDIA NVENC detected. Using GPU encoding.");
            }

            string videoCodec = useGpu
                ? gpuVideoCodec
                : cpuVideoCodec;


            string outputPath = Path.Combine(
                outputFolder,
                Path.GetFileNameWithoutExtension(inputVideo) +
                "-edited" + extension);

            if (string.Equals(inputVideo, outputPath,
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The output must differ from the input.");
                return;
            }

            if (File.Exists(outputPath))
            {
                DialogResult answer = MessageBox.Show(
                    "The output file already exists:\r\n" +
                    outputPath + "\r\n\r\nOverwrite it?",
                    "Confirm Overwrite",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (answer != DialogResult.Yes)
                    return;
            }

            btnProcess.Enabled = false;
            progressBar1.Value = 0;

            try
            {
                LogMessage("Starting processing...");
                LogMessage("Input: " + inputVideo);
                LogMessage("Output: " + outputPath);

                // Probe the video duration.
                ProcessStartInfo probe = new ProcessStartInfo();
                probe.FileName = "ffprobe";
                probe.Arguments =
                    "-v error -show_entries format=duration " +
                    "-of default=noprint_wrappers=1:nokey=1 " +
                    "\"" + inputVideo + "\"";
                probe.UseShellExecute = false;
                probe.RedirectStandardOutput = true;
                probe.RedirectStandardError = true;
                probe.CreateNoWindow = true;

                double duration;
                string durationText;
                string probeError;
                int probeExitCode;

                using (Process probeProcess = new Process())
                {
                    probeProcess.StartInfo = probe;
                    probeProcess.Start();

                    Task<string> outputTask =
                        probeProcess.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask =
                        probeProcess.StandardError.ReadToEndAsync();

                    await Task.Run(() => probeProcess.WaitForExit());

                    durationText = await outputTask;
                    probeError = await errorTask;
                    probeExitCode = probeProcess.ExitCode;
                }

                if (probeExitCode != 0 ||
                    !double.TryParse(
                        durationText.Trim(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out duration))
                {
                    throw new Exception(
                        "Could not read video duration. " + probeError);
                }


                // Probe the video width and height.
                ProcessStartInfo resolutionProbe = new ProcessStartInfo();
                resolutionProbe.FileName = "ffprobe";
                resolutionProbe.Arguments =
                    "-v error -select_streams v:0 " +
                    "-show_entries stream=width,height " +
                    "-of csv=s=x:p=0 " +
                    "\"" + inputVideo + "\"";

                resolutionProbe.UseShellExecute = false;
                resolutionProbe.RedirectStandardOutput = true;
                resolutionProbe.RedirectStandardError = true;
                resolutionProbe.CreateNoWindow = true;

                int videoWidth;
                int videoHeight;

                using (Process probeProcess = new Process())
                {
                    probeProcess.StartInfo = resolutionProbe;
                    probeProcess.Start();

                    Task<string> outputTask =
                        probeProcess.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask =
                        probeProcess.StandardError.ReadToEndAsync();

                    await Task.Run(() => probeProcess.WaitForExit());

                    string resolutionText = await outputTask;
                    string resolutionError = await errorTask;

                    string firstLine = resolutionText
                                        .Split(new[] { '\r', '\n' },
                                            StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault();

                    string[] dimensions = (firstLine ?? "").Trim().Split('x');

                    int width;
                    int height;

                    LogMessage("FFprobe exit code: " + probeProcess.ExitCode);
                    LogMessage("FFprobe output: [" + resolutionText + "]");
                    LogMessage("FFprobe error: [" + resolutionError + "]");

                    if (probeProcess.ExitCode != 0 ||
                        dimensions.Length != 2 ||
                        !int.TryParse(dimensions[0], out width) ||
                        !int.TryParse(dimensions[1], out height) ||
                        width <= 0 || height <= 0)
                    {
                        throw new Exception(
                            "Could not read video resolution. " +
                            resolutionError);
                    }

                    videoWidth = width;
                    videoHeight = height;
                }

                LogMessage(
                    "Video resolution: " +
                    videoWidth + "x" + videoHeight);


                // Validate every interval.
                List<VideoInterval> intervals =
                    GetIntervals(duration);

                // Build one enable expression covering all intervals.
                StringBuilder enableExpression = new StringBuilder();

                for (int i = 0; i < intervals.Count; i++)
                {
                    if (i > 0)
                        enableExpression.Append("+");

                    string start = intervals[i].Start.TotalSeconds.ToString(
                        CultureInfo.InvariantCulture);
                    string end = intervals[i].End.TotalSeconds.ToString(
                        CultureInfo.InvariantCulture);

                    enableExpression.Append(
                        "between(t," + start + "," + end + ")");
                }

                string enable = enableExpression.ToString();

                string videoFilter =
                    "[1:v]scale=" + videoWidth + ":" + videoHeight +
                    ",setsar=1[img];" +
                    "[0:v][img]overlay=x=0:y=0:enable='" +
                    enable + "',format=yuv420p[v]";

                string audioFilter =
                    "volume=volume=0:enable='" + enable + "'";

                // Construct arguments for the installed .NET Framework /
                // C# 7.3 environment (no ArgumentList required).
                ProcessStartInfo ffmpeg = new ProcessStartInfo();
                ffmpeg.FileName = "ffmpeg";
                ffmpeg.UseShellExecute = false;
                ffmpeg.RedirectStandardOutput = true;
                ffmpeg.RedirectStandardError = true;
                ffmpeg.CreateNoWindow = true;

                StringBuilder arguments = new StringBuilder();

                arguments.Append("-hide_banner -y ");
                arguments.Append("-i \"" + inputVideo + "\" ");
                arguments.Append("-loop 1 -framerate 30 ");
                arguments.Append("-i \"" + imagePath + "\" ");
                arguments.Append("-filter_complex \"");
                arguments.Append(videoFilter);
                arguments.Append("\" ");
                arguments.Append("-map \"[v]\" -map \"0:a?\" ");
                arguments.Append("-af \"" + audioFilter + "\" ");
                arguments.Append("-c:v " + videoCodec + " ");


                if (videoCodec == "h264_nvenc")
                {
                    // NVIDIA hardware H.264 encoding
                    arguments.Append("-preset p5 -rc vbr -cq 20 -b:v 0 ");
                }
                else if (videoCodec == "libx264")
                {
                    // CPU H.264 encoding
                    arguments.Append("-preset medium -crf 20 ");
                }
                else if (videoCodec == "mpeg4")
                {
                    arguments.Append("-q:v 3 ");
                }
                else if (videoCodec == "libvpx-vp9")
                {
                    arguments.Append("-crf 30 -b:v 0 ");
                }


                arguments.Append("-c:a " + audioCodec + " ");
                arguments.Append("-b:a " + audioBitrate + " ");
                arguments.Append("-t " +
    duration.ToString(CultureInfo.InvariantCulture) + " ");

                // faststart is intended for MP4/MOV-style containers,
                // not MTS/M2TS, AVI, MKV, or WebM.
                string lowerExtension = extension.ToLowerInvariant();

                if (lowerExtension == ".mp4" ||
                    lowerExtension == ".m4v" ||
                    lowerExtension == ".mov")
                {
                    arguments.Append("-movflags +faststart ");
                }

                arguments.Append("-progress pipe:1 -nostats ");
                arguments.Append("\"" + outputPath + "\"");

                ffmpeg.Arguments = arguments.ToString();

                LogMessage("Intervals: " + intervals.Count);

                for (int i = 0; i < intervals.Count; i++)
                {
                    LogMessage(
                        "Interval " + (i + 1) + ": " +
                        intervals[i].Start.ToString(@"hh\:mm\:ss") +
                        " - " +
                        intervals[i].End.ToString(@"hh\:mm\:ss"));
                }

                LogMessage("Starting FFmpeg...");

                using (Process process = new Process())
                {
                    process.StartInfo = ffmpeg;

                    process.OutputDataReceived += (s, args) =>
                    {
                        if (args.Data == null)
                            return;

                        string line = args.Data;

                        if (line.StartsWith("out_time_us="))
                        {
                            long microseconds;

                            if (long.TryParse(
                                line.Substring("out_time_us=".Length),
                                out microseconds))
                            {
                                int percent = (int)Math.Min(
                                    100,
                                    Math.Max(
                                        0,
                                        microseconds / (duration * 1000000.0) * 100));

                                try
                                {
                                    UpdateProgress(percent);
                                }
                                catch (InvalidOperationException)
                                {
                                    // Form may be closing.
                                }
                            }
                        }
                        else if (line == "progress=end")
                        {
                            try
                            {
                                UpdateProgress(100);
                            }
                            catch (InvalidOperationException)
                            {
                                // Form may be closing.
                            }
                        }
                    };

                    process.ErrorDataReceived += (s, args) =>
                    {
                        if (args.Data != null)
                        {
                            try
                            {
                                LogMessage(args.Data);
                            }
                            catch (InvalidOperationException)
                            {
                                // Form may be closing.
                            }
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Run(() => process.WaitForExit());
                    process.WaitForExit();

                    if (process.ExitCode == 0 && File.Exists(outputPath))
                    {
                        UpdateProgress(100);
                        LogMessage("Processing completed successfully.");
                        LogMessage("Saved: " + outputPath);

                        MessageBox.Show(
                            "Edited video saved to:\r\n" + outputPath,
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        LogMessage("FFmpeg failed. Check the log for details.");

                        MessageBox.Show(
                            "FFmpeg could not process the video. " +
                            "Check the log for details.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Processing Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnProcess.Enabled = true;
            }
        }
    }
}

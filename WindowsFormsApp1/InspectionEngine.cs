using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Inspection
{
    /// <summary>
    /// Engine untuk execute inspection berdasarkan configuration
    /// Ini adalah CORE RUNTIME yang menjalankan inspection tanpa coding
    /// </summary>
    public class InspectionEngine
    {
        private InspectionProject project;
        private Bitmap currentImage;

        public event EventHandler<InspectionProgressEventArgs> ProgressChanged;
        public event EventHandler<InspectionStepResult> StepCompleted;

        public InspectionEngine(InspectionProject inspectionProject)
        {
            project = inspectionProject ?? throw new ArgumentNullException(nameof(inspectionProject));
        }

        /// <summary>
        /// Execute full inspection project
        /// </summary>
        public async Task<InspectionResult> ExecuteInspectionAsync(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            currentImage = image;

            var result = new InspectionResult
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                StartTime = DateTime.Now,
                OriginalImage = image,
                OverallPassed = true
            };

            Stopwatch totalTimer = Stopwatch.StartNew();

            try
            {
                // Execute each step in order
                var sortedSteps = project.Steps
                    .Where(s => s.IsEnabled)
                    .OrderBy(s => s.StepOrder)
                    .ToList();

                int totalSteps = sortedSteps.Count;
                int currentStepIndex = 0;

                foreach (var step in sortedSteps)
                {
                    currentStepIndex++;

                    // Report progress
                    OnProgressChanged(new InspectionProgressEventArgs
                    {
                        CurrentStep = currentStepIndex,
                        TotalSteps = totalSteps,
                        StepName = step.StepName,
                        Message = $"Executing: {step.StepName}"
                    });

                    // Execute step
                    var stepResult = await ExecuteStepAsync(step, image);
                    result.StepResults.Add(stepResult);

                    // Notify step completed
                    OnStepCompleted(stepResult);

                    // Check if failed and critical
                    if (!stepResult.Passed && step.PassCriteria.IsCritical)
                    {
                        result.OverallPassed = false;
                        result.ResultSummary = $"FAILED at critical step: {step.StepName}";
                        break; // Stop inspection
                    }

                    if (!stepResult.Passed)
                    {
                        result.OverallPassed = false;
                    }
                }

                // Final summary
                if (result.OverallPassed)
                {
                    result.ResultSummary = $"PASSED - All {result.StepResults.Count} steps completed successfully";
                }
                else if (string.IsNullOrEmpty(result.ResultSummary))
                {
                    int failedCount = result.StepResults.Count(r => !r.Passed);
                    result.ResultSummary = $"FAILED - {failedCount} of {result.StepResults.Count} steps failed";
                }
            }
            catch (Exception ex)
            {
                result.OverallPassed = false;
                result.ResultSummary = $"ERROR: {ex.Message}";
            }
            finally
            {
                totalTimer.Stop();
                result.EndTime = DateTime.Now;
                result.TotalProcessingTimeMs = totalTimer.Elapsed.TotalMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// Execute single inspection step
        /// </summary>
        private async Task<InspectionStepResult> ExecuteStepAsync(InspectionStep step, Bitmap image)
        {
            Stopwatch stepTimer = Stopwatch.StartNew();

            var stepResult = new InspectionStepResult
            {
                StepId = step.StepId,
                StepName = step.StepName,
                ExecutionTime = DateTime.Now,
                ExpectedValue = step.PassCriteria.ExpectedValue
            };

            try
            {
                // Extract ROI from image
                Bitmap roiImage = ExtractROI(image, step.ROI);

                // Execute based on inspection type
                switch (step.InspectionType)
                {
                    case InspectionType.LabelOCR:
                        await ExecuteLabelOcrAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.ScrewDetection:
                        await ExecuteScrewDetectionAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.BarcodeReading:
                        await ExecuteBarcodeReadingAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.ColorDetection:
                        await ExecuteColorDetectionAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.DefectDetection:
                        await ExecuteDefectDetectionAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.Measurement:
                        await ExecuteMeasurementAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.PresenceCheck:
                        await ExecutePresenceCheckAsync(step, roiImage, stepResult);
                        break;

                    case InspectionType.AlignmentCheck:
                        await ExecuteAlignmentCheckAsync(step, roiImage, stepResult);
                        break;

                    default:
                        throw new NotImplementedException($"Inspection type {step.InspectionType} not implemented");
                }

                // Evaluate pass/fail based on criteria
                stepResult.Passed = EvaluatePassFail(step.PassCriteria, stepResult.ActualValue, stepResult.ExpectedValue);

                if (!stepResult.Passed && string.IsNullOrEmpty(stepResult.ErrorMessage))
                {
                    stepResult.ErrorMessage = $"Value mismatch: Expected '{stepResult.ExpectedValue}', Got '{stepResult.ActualValue}'";
                }
            }
            catch (Exception ex)
            {
                stepResult.Passed = false;
                stepResult.ErrorMessage = $"Execution error: {ex.Message}";
                stepResult.Confidence = 0;
            }
            finally
            {
                stepTimer.Stop();
                stepResult.ProcessingTimeMs = stepTimer.Elapsed.TotalMilliseconds;
            }

            return stepResult;
        }

        // ============================================================
        //  INSPECTION TYPE IMPLEMENTATIONS
        // ============================================================

        private async Task ExecuteLabelOcrAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            // TODO: Integrate dengan Python AI OCR model
            // Untuk sekarang, simulasi result
            await Task.Delay(100); // Simulate processing

            var config = step.LabelOcrConfig;

            // PLACEHOLDER: Ini akan diganti dengan actual OCR call ke Python
            // Example: var ocrResult = await CallPythonOcrApi(roi, config);

            string detectedText = "SAMPLE_LABEL_123"; // Dummy result
            result.ActualValue = detectedText;
            result.Confidence = 0.92;

            Debug.WriteLine($"[OCR] Expected: {config.ExpectedText}, Got: {detectedText}");
        }

        private async Task ExecuteScrewDetectionAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(150); // Simulate processing

            var config = step.ScrewDetectionConfig;

            // PLACEHOLDER: Akan diganti dengan YOLOv8 detection
            int detectedCount = 4; // Dummy result
            result.ActualValue = detectedCount.ToString();
            result.ExpectedValue = config.ExpectedCount.ToString();
            result.Confidence = 0.87;

            Debug.WriteLine($"[Screw Detection] Expected: {config.ExpectedCount}, Got: {detectedCount}");
        }

        private async Task ExecuteBarcodeReadingAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(80); // Simulate processing

            var config = step.BarcodeReadingConfig;

            // PLACEHOLDER: Akan diganti dengan barcode reader library
            string barcodeValue = "123456789012"; // Dummy result
            result.ActualValue = barcodeValue;
            result.Confidence = 0.95;

            Debug.WriteLine($"[Barcode] Type: {config.BarcodeType}, Got: {barcodeValue}");
        }

        private async Task ExecuteColorDetectionAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(50); // Simulate processing

            var config = step.ColorDetectionConfig;

            // PLACEHOLDER: Akan diganti dengan actual color analysis
            Color dominantColor = GetDominantColor(roi);
            result.ActualValue = $"RGB({dominantColor.R},{dominantColor.G},{dominantColor.B})";
            result.ExpectedValue = $"RGB({config.ExpectedColorRGB.R},{config.ExpectedColorRGB.G},{config.ExpectedColorRGB.B})";
            result.Confidence = 0.88;

            Debug.WriteLine($"[Color] Expected: {config.ExpectedColorName}, Got: {result.ActualValue}");
        }

        private async Task ExecuteDefectDetectionAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(200); // Simulate processing

            var config = step.DefectDetectionConfig;

            // PLACEHOLDER: Akan diganti dengan AI defect detection
            int defectCount = 0; // Dummy result
            result.ActualValue = defectCount.ToString();
            result.ExpectedValue = config.MaxAllowedDefects.ToString();
            result.Confidence = 0.91;

            Debug.WriteLine($"[Defect Detection] Max Allowed: {config.MaxAllowedDefects}, Found: {defectCount}");
        }

        private async Task ExecuteMeasurementAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(120); // Simulate processing

            var config = step.MeasurementConfig;

            // PLACEHOLDER: Akan diganti dengan actual measurement
            double measuredValue = 25.4; // Dummy result in mm
            result.ActualValue = $"{measuredValue} {config.Unit}";
            result.ExpectedValue = $"{config.ExpectedValue} {config.Unit}";
            result.Confidence = 0.89;

            Debug.WriteLine($"[Measurement] {config.MeasurementType}: Expected {config.ExpectedValue}, Got {measuredValue}");
        }

        private async Task ExecutePresenceCheckAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(60); // Simulate processing

            // Simple presence check based on pixel intensity
            bool isPresent = CheckPresence(roi);
            result.ActualValue = isPresent ? "Present" : "Absent";
            result.ExpectedValue = "Present";
            result.Confidence = 0.93;
        }

        private async Task ExecuteAlignmentCheckAsync(InspectionStep step, Bitmap roi, InspectionStepResult result)
        {
            await Task.Delay(100); // Simulate processing

            // Dummy alignment check
            result.ActualValue = "Aligned";
            result.ExpectedValue = "Aligned";
            result.Confidence = 0.90;
        }

        // ============================================================
        //  HELPER METHODS
        // ============================================================

        private Bitmap ExtractROI(Bitmap source, RegionOfInterest roi)
        {
            try
            {
                // Validate ROI bounds
                int x = Math.Max(0, Math.Min(roi.X, source.Width - 1));
                int y = Math.Max(0, Math.Min(roi.Y, source.Height - 1));
                int w = Math.Min(roi.Width, source.Width - x);
                int h = Math.Min(roi.Height, source.Height - y);

                Rectangle cropRect = new Rectangle(x, y, w, h);
                return source.Clone(cropRect, source.PixelFormat);
            }
            catch
            {
                // Return original if ROI extraction fails
                return source;
            }
        }

        private bool EvaluatePassFail(PassFailCriteria criteria, string actualValue, string expectedValue)
        {
            if (string.IsNullOrEmpty(actualValue))
                return false;

            try
            {
                switch (criteria.Operator)
                {
                    case ComparisonOperator.Equals:
                        return actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);

                    case ComparisonOperator.NotEquals:
                        return !actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);

                    case ComparisonOperator.Contains:
                        return actualValue.Contains(expectedValue);

                    case ComparisonOperator.NotContains:
                        return !actualValue.Contains(expectedValue);

                    case ComparisonOperator.GreaterThan:
                        if (double.TryParse(actualValue, out double actual) &&
                            double.TryParse(expectedValue, out double expected))
                        {
                            return actual > expected - criteria.Tolerance;
                        }
                        break;

                    case ComparisonOperator.LessThan:
                        if (double.TryParse(actualValue, out actual) &&
                            double.TryParse(expectedValue, out expected))
                        {
                            return actual < expected + criteria.Tolerance;
                        }
                        break;

                    case ComparisonOperator.InRange:
                        // Expected format: "min,max"
                        var parts = expectedValue.Split(',');
                        if (parts.Length == 2 &&
                            double.TryParse(actualValue, out actual) &&
                            double.TryParse(parts[0], out double min) &&
                            double.TryParse(parts[1], out double max))
                        {
                            return actual >= min && actual <= max;
                        }
                        break;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private Color GetDominantColor(Bitmap image)
        {
            // Simple dominant color calculation
            long r = 0, g = 0, b = 0;
            int count = 0;

            for (int x = 0; x < image.Width; x += 5) // Sample every 5 pixels for speed
            {
                for (int y = 0; y < image.Height; y += 5)
                {
                    Color pixel = image.GetPixel(x, y);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    count++;
                }
            }

            if (count == 0) return Color.Black;

            return Color.FromArgb((int)(r / count), (int)(g / count), (int)(b / count));
        }

        private bool CheckPresence(Bitmap roi)
        {
            // Simple presence check: if average brightness > threshold
            int brightness = 0;
            int count = 0;

            for (int x = 0; x < roi.Width; x += 5)
            {
                for (int y = 0; y < roi.Height; y += 5)
                {
                    Color pixel = roi.GetPixel(x, y);
                    brightness += (pixel.R + pixel.G + pixel.B) / 3;
                    count++;
                }
            }

            int avgBrightness = count > 0 ? brightness / count : 0;
            return avgBrightness > 30; // Threshold
        }

        // Event raisers
        protected virtual void OnProgressChanged(InspectionProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnStepCompleted(InspectionStepResult e)
        {
            StepCompleted?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Event args untuk progress inspection
    /// </summary>
    public class InspectionProgressEventArgs : EventArgs
    {
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string StepName { get; set; }
        public string Message { get; set; }
        public int ProgressPercentage => TotalSteps > 0 ? (int)((double)CurrentStep / TotalSteps * 100) : 0;
    }
}
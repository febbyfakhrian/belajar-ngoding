using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Inspection
{
    /// <summary>
    /// Root configuration untuk satu inspection project
    /// </summary>
    public class InspectionProject
    {
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Description { get; set; }
        public List<InspectionStep> Steps { get; set; } = new List<InspectionStep>();

        public InspectionProject()
        {
            ProjectId = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }

    /// <summary>
    /// Satu step inspection (bisa Label OCR, Screw Detection, dll)
    /// </summary>
    public class InspectionStep
    {
        public string StepId { get; set; }
        public string StepName { get; set; }
        public int StepOrder { get; set; }
        public bool IsEnabled { get; set; } = true;

        public InspectionType InspectionType { get; set; }

        // Region of Interest (area yang di-inspect)
        public RegionOfInterest ROI { get; set; }

        // Parameter spesifik per jenis inspection
        public LabelOcrConfig LabelOcrConfig { get; set; }
        public ScrewDetectionConfig ScrewDetectionConfig { get; set; }
        public BarcodeReadingConfig BarcodeReadingConfig { get; set; }
        public ColorDetectionConfig ColorDetectionConfig { get; set; }
        public DefectDetectionConfig DefectDetectionConfig { get; set; }
        public MeasurementConfig MeasurementConfig { get; set; }

        // Pass/Fail criteria
        public PassFailCriteria PassCriteria { get; set; }

        public InspectionStep()
        {
            StepId = Guid.NewGuid().ToString();
            ROI = new RegionOfInterest();
            PassCriteria = new PassFailCriteria();
        }
    }

    /// <summary>
    /// Jenis-jenis inspection yang supported
    /// </summary>
    public enum InspectionType
    {
        LabelOCR,           // Baca text dari label
        ScrewDetection,     // Hitung jumlah screw
        BarcodeReading,     // Scan barcode/QR
        ColorDetection,     // Deteksi warna
        DefectDetection,    // Deteksi cacat (scratch, dent, dll)
        Measurement,        // Ukur dimensi
        PresenceCheck,      // Ada/tidak ada komponen
        AlignmentCheck      // Cek posisi/alignment
    }

    /// <summary>
    /// Region of Interest - area yang akan di-inspect
    /// </summary>
    public class RegionOfInterest
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonIgnore]
        public Rectangle Rectangle => new Rectangle(X, Y, Width, Height);

        public void SetFromRectangle(Rectangle rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }
    }

    /// <summary>
    /// Config untuk Label OCR Inspection
    /// </summary>
    public class LabelOcrConfig
    {
        public string ExpectedText { get; set; }
        public double MinConfidence { get; set; } = 0.8;  // 0.0 - 1.0
        public bool CaseSensitive { get; set; } = false;
        public bool AllowPartialMatch { get; set; } = false;
        public string OcrEngine { get; set; } = "Tesseract"; // Tesseract, EasyOCR, dll
        public string Language { get; set; } = "eng";
    }

    /// <summary>
    /// Config untuk Screw Detection
    /// </summary>
    public class ScrewDetectionConfig
    {
        public int ExpectedCount { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public double MinConfidence { get; set; } = 0.7;
        public string DetectionModel { get; set; } = "YOLOv8"; // Model AI yang dipakai
        public double MinScrewSize { get; set; } = 10; // pixel
        public double MaxScrewSize { get; set; } = 100; // pixel
    }

    /// <summary>
    /// Config untuk Barcode Reading
    /// </summary>
    public class BarcodeReadingConfig
    {
        public BarcodeType BarcodeType { get; set; } = BarcodeType.QRCode;
        public string ExpectedPattern { get; set; } // Regex pattern
        public bool ValidateChecksum { get; set; } = true;
        public List<string> AllowedValues { get; set; } = new List<string>();
    }

    public enum BarcodeType
    {
        QRCode,
        DataMatrix,
        Code128,
        Code39,
        EAN13,
        UPC,
        Any
    }

    /// <summary>
    /// Config untuk Color Detection
    /// </summary>
    public class ColorDetectionConfig
    {
        public string ExpectedColorName { get; set; }
        public ColorRGB ExpectedColorRGB { get; set; }
        public double ColorTolerance { get; set; } = 20; // Delta E tolerance
        public double MinColorPercentage { get; set; } = 80; // % area yang harus match
        public string ColorSpace { get; set; } = "RGB"; // RGB, HSV, LAB
    }

    public class ColorRGB
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        [JsonIgnore]
        public Color Color => Color.FromArgb(R, G, B);

        public void SetFromColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }
    }

    /// <summary>
    /// Config untuk Defect Detection
    /// </summary>
    public class DefectDetectionConfig
    {
        public List<DefectType> DefectTypes { get; set; } = new List<DefectType>();
        public double MinDefectSize { get; set; } = 5; // pixel
        public double MaxAllowedDefects { get; set; } = 0; // Max defect yang diperbolehkan
        public double MinConfidence { get; set; } = 0.75;
        public string DetectionModel { get; set; } = "YOLOv8-Defect";
    }

    public enum DefectType
    {
        Scratch,
        Dent,
        Crack,
        Stain,
        Bubble,
        Discoloration,
        MissingPart,
        ExcessMaterial
    }

    /// <summary>
    /// Config untuk Measurement
    /// </summary>
    public class MeasurementConfig
    {
        public MeasurementType MeasurementType { get; set; }
        public double ExpectedValue { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string Unit { get; set; } = "mm";
        public double PixelToUnitRatio { get; set; } = 1.0; // Calibration: pixel to mm
    }

    public enum MeasurementType
    {
        Length,
        Width,
        Height,
        Diameter,
        Area,
        Angle
    }

    /// <summary>
    /// Pass/Fail Criteria untuk setiap step
    /// </summary>
    public class PassFailCriteria
    {
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.Equals;
        public string ExpectedValue { get; set; }
        public double Tolerance { get; set; } = 0;
        public bool IsCritical { get; set; } = true; // Jika true, fail = stop inspection
    }

    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        NotContains,
        InRange,
        Matches // Regex
    }

    /// <summary>
    /// Result dari satu inspection step
    /// </summary>
    public class InspectionStepResult
    {
        public string StepId { get; set; }
        public string StepName { get; set; }
        public bool Passed { get; set; }
        public string ActualValue { get; set; }
        public string ExpectedValue { get; set; }
        public double Confidence { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ExecutionTime { get; set; }
        public double ProcessingTimeMs { get; set; }

        // Optional: Image result untuk visualisasi
        [JsonIgnore]
        public Bitmap ResultImage { get; set; }

        public string ResultImageBase64 { get; set; } // Untuk save ke JSON
    }

    /// <summary>
    /// Result keseluruhan inspection project
    /// </summary>
    public class InspectionResult
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double TotalProcessingTimeMs { get; set; }
        public bool OverallPassed { get; set; }
        public List<InspectionStepResult> StepResults { get; set; } = new List<InspectionStepResult>();

        [JsonIgnore]
        public Bitmap OriginalImage { get; set; }

        public string OriginalImagePath { get; set; }
        public string ResultSummary { get; set; }
    }
}
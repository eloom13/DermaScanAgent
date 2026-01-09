using System.ComponentModel;

namespace AiAgents.SkinCancerAgent.Domain.Enums;

public enum TaskType
{
    SkinLesionAnalysis = 1,
    HealthCheck = 2
}

public enum SampleStatus
{
    Queued = 0,         // Tek uploadovano
    Processing = 1,     // Agent obrađuje
    Scored = 2,         // Model dao predikciju
    PendingReview = 3,  // Nesiguran, čeka doktora
    Reviewed = 4,       // Doktor potvrdio (Gold Data)
    Failed = 99
}

public enum Decision
{
    None = 0,
    AutoAccept = 1,     // Model je >85% siguran
    PendingReview = 2,  // Model nije siguran
    Alert = 3,          // Detektovano maligno oboljenje (npr. Melanoma)
    AutoReject = 4
}

public enum DiagnosisType
{
    [Description("Nepoznato")]
    Unknown = 0,

    // 1. akiec: Actinic keratoses / Bowen's disease (Prekancerozno/Karcinom)
    [Description("akiec")]
    ActinicKeratoses = 1,

    // 2. bcc: Basal cell carcinoma (Karcinom)
    [Description("bcc")]
    BasalCellCarcinoma = 2,

    // 3. bkl: Benign keratosis-like lesions (Benigno)
    [Description("bkl")]
    BenignKeratosis = 3,

    // 4. df: Dermatofibroma (Benigno)
    [Description("df")]
    Dermatofibroma = 4,

    // 5. mel: Melanoma (Maligno - OPASNO!)
    [Description("mel")]
    Melanoma = 5,

    // 6. nv: Melanocytic nevi (Mladeži - Benigno)
    [Description("nv")]
    MelanocyticNevi = 6,

    // 7. vasc: Vascular lesions (Benigno)
    [Description("vasc")]
    VascularLesion = 7
}
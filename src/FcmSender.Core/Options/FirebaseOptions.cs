namespace FcmSender.Core.Options;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     Strongly typed configuration for Firebase Cloud Messaging integration.
/// </summary>
public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    /// <summary>
    ///     Firebase 프로젝트 ID (예: my-firebase-project).
    /// </summary>
    [Required]
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    ///     요청 시 토큰이 생략되었을 때 사용할 기본 디바이스 토큰(선택).
    /// </summary>
    public string? DefaultDeviceToken { get; set; }

    /// <summary>
    ///     서비스 계정 JSON 로딩에 사용할 자격 증명 옵션.
    /// </summary>
    public FirebaseCredentialOptions Credentials { get; set; } = new();

    public class FirebaseCredentialOptions
    {
        /// <summary>
        ///     서비스 계정 JSON 파일 절대/상대 경로.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        ///     서비스 계정 JSON 원본 문자열.
        /// </summary>
        public string? Json { get; set; }

        /// <summary>
        ///     Base64 인코딩된 서비스 계정 JSON 문자열.
        /// </summary>
        public string? JsonBase64 { get; set; }
    }
}

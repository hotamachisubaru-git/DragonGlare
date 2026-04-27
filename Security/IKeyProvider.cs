namespace DragonGlare.Core.Security;

/// <summary>
/// 暗号化および署名用の鍵を提供するインターフェース
/// </summary>
public interface IKeyProvider
{
    /// <summary>AES暗号化用の256ビット鍵を取得します</summary>
    byte[] GetEncryptionKey();

    /// <summary>HMAC署名用の鍵を取得します</summary>
    byte[] GetHmacKey();
}
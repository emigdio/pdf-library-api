using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

public sealed class R2Storage
{
    private readonly string _bucket;
    private readonly string _prefix;
    private readonly IAmazonS3 _s3;

    public R2Storage(IConfiguration cfg)
    {
        var accountId = cfg["R2_ACCOUNT_ID"] ?? throw new Exception("Missing R2_ACCOUNT_ID");
        var accessKey = cfg["R2_ACCESS_KEY_ID"] ?? throw new Exception("Missing R2_ACCESS_KEY_ID");
        var secretKey = cfg["R2_SECRET_ACCESS_KEY"] ?? throw new Exception("Missing R2_SECRET_ACCESS_KEY");

        _bucket = cfg["R2_BUCKET"] ?? throw new Exception("Missing R2_BUCKET");
        _prefix = cfg["R2_PREFIX"] ?? "books/";

        var endpoint = $"https://{accountId}.r2.cloudflarestorage.com"; // R2 S3 endpoint :contentReference[oaicite:4]{index=4}

        var creds = new BasicAWSCredentials(accessKey, secretKey);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true, // muy útil con endpoints S3 compatibles
        };

        _s3 = new AmazonS3Client(creds, s3Config);
    }

    public async Task<List<string>> ListPdfKeysAsync(CancellationToken ct = default)
    {
        var keys = new List<string>();
        string? token = null;

        do
        {
            var resp = await _s3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _bucket,
                Prefix = _prefix,
                ContinuationToken = token
            }, ct);

            keys.AddRange(resp.S3Objects
                .Where(o => o.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                .Select(o => o.Key));

            token = resp.IsTruncated == true ? resp.NextContinuationToken : null;

        } while (token != null);

        return keys;
    }

    public string GetPresignedDownloadUrl(string key, TimeSpan expiresIn)
    {
        // Presigned URLs: patrón estándar en S3 :contentReference[oaicite:5]{index=5}
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiresIn)
        };

        return _s3.GetPreSignedURL(req);
    }

    public string KeyForId(string id) => $"{_prefix}{id}.pdf";
}
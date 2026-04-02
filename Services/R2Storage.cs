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
        _prefix = cfg["R2_PREFIX"] ?? "";

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

    public async Task<Stream> DownloadFileAsync(string key, CancellationToken ct)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucket,
            Key = key
        };
        var response = await _s3.GetObjectAsync(request, ct);
        return response.ResponseStream; // Retorna el contenido del PDF
    }

    public async Task<string> UploadStreamAsync(string key, Stream stream, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket, // El nombre de tu bucket de R2
            Key = key,                // La ruta (ej: "thumbnails/archivo.png")
            InputStream = stream,      // El flujo de datos
            ContentType = contentType, // Importante para que el navegador sepa qué es (image/png, application/pdf)
            DisablePayloadSigning = true // Requerido por R2 para subidas directas
        };

        var response = await _s3.PutObjectAsync(request, ct);

        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            return key;
        }

        throw new Exception($"Error al subir el archivo {key} a R2");
    }

    public string KeyForId(string id) => $"{_prefix}{id}.pdf";
}
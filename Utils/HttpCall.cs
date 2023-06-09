﻿using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using NLog;
using RestSharp;
using RestSharp.Serializers.Json;

namespace Utils;

public class HttpResponse<T>
{
    public HttpResponse(T response, Uri requestUri)
    {
        _response = response;
        RequestUri = requestUri;
        HasResponse = true;
    }

    public HttpResponse(Uri requestUri)
    {
        RequestUri = requestUri;
        HasResponse = false;
    }

    public Uri RequestUri { get; }

    private readonly T _response = default!;

    public T Response
    {
        get
        {
            if (!HasResponse)
                throw new MemberAccessException("Use HasResponse to check availability Response field");
            return _response;
        }
    }

    public bool HasResponse { get; }
}

public class HttpCall
{
    public static async Task<HttpResponse<T>> Get<T>(Uri uri, int timeoutMs = 300,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var options = new RestClientOptions
        {
            ThrowOnAnyError = true,
            MaxTimeout = timeoutMs
        };
        var restClient = new RestClient(options);
        var restRequest = new RestRequest(uri);
        var sw = new Stopwatch();
        Log.Trace($"Request to {uri} started");
        sw.Start();
        try
        {
            var response = await restClient.GetAsync(restRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Fatal($"Request to {uri} failed with code {response.StatusCode}");
                return new HttpResponse<T>(uri);
            }

            if (response.ContentLength == 0)
            {
                Log.Warn($"Request to {uri} return empty response");
                return new HttpResponse<T>(uri);
            }

            try
            {
                var result = (response.ContentType) switch
                {
                    "application/xml" => DeserializeXml<T>(response.RawBytes!),
                    "application/json" =>
                        DeserializeJson<T>(response!, jsonSerializerOptions),
                    _ =>
                        throw new Exception($"Unknown ContentType: {response.ContentType}")
                };
                if (result is null)
                {
                    Log.Fatal(
                        $"Unable to parse response of {uri} as provided type {typeof(T)}. Response: {response.Content}");
                    return new HttpResponse<T>(uri);
                }

                return new HttpResponse<T>(result, uri);
            }
            catch (Exception e)
            {
                Log.Fatal(e,
                    $"Unable to parse response of {uri} as provided type {typeof(T)}. Response: {response.Content}");
                throw;
            }
        }
        catch (Exception e)
        {
            Log.Fatal(e, $"Unable to process request to {uri}. Exception happened {e}");
            return new HttpResponse<T>(uri);
        }
        finally
        {
            sw.Stop();
            Log.Trace($"Request to {uri} finished in {sw.Elapsed}");
        }
    }

    private static T? DeserializeXml<T>(byte[] bytes)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            using var sr = new StreamReader(ms, Encoding.GetEncoding(1251));
            var xmlSerializer1 = new XmlSerializer(typeof(T));
            var deserialized = (T?)xmlSerializer1.Deserialize(sr);
            return deserialized;
        }
        catch (Exception e)
        {
            Log.Error(e, $"Error occured on deserializaton of xml.");
            return default;
        }
    }

    private static T? DeserializeJson<T>(RestResponse restResponse, JsonSerializerOptions? options)
    {
        try
        {
            var serializer = options == null ? new SystemTextJsonSerializer() : new SystemTextJsonSerializer(options);
            return serializer.Deserialize<T>(restResponse);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Error occured on deserializaton of json.");
            return default;
        }
    }

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
}
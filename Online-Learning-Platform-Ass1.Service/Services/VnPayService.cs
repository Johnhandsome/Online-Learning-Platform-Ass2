using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Online_Learning_Platform_Ass1.Service.DTOs.Payment;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class VnPayService (IConfiguration configuration) : IVnPayService
{
    private readonly IConfiguration _configuration = configuration;

    public string CreatePaymentUrl(string ipAddress, VnPayRequestModel model)
    {
        var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        var pay = new VnPayLibrary();
        var urlCallBack = _configuration["VnPay:PaymentBackReturnUrl"];

        pay.AddRequestData("vnp_Version", _configuration["VnPay:Version"] ?? "2.1.0");
        pay.AddRequestData("vnp_Command", _configuration["VnPay:Command"] ?? "pay");
        pay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"] ?? "");
        pay.AddRequestData("vnp_Amount", ((long)(model.Amount * 100)).ToString()); 
        pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
        pay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"] ?? "VND");
        pay.AddRequestData("vnp_IpAddr", ipAddress);
        pay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"] ?? "vn");
        pay.AddRequestData("vnp_OrderInfo", $"{model.FullName} ({model.Description})");
        pay.AddRequestData("vnp_OrderType", "other");
        pay.AddRequestData("vnp_ReturnUrl", urlCallBack ?? "https://localhost:7214/Payment/PaymentCallback");
        pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString()); 

        var paymentUrl =
            pay.CreateRequestUrl(_configuration["VnPay:BaseUrl"] ?? "", _configuration["VnPay:HashSecret"] ?? "");

        return paymentUrl;
    }

    public PaymentResponseModel PaymentExecute(IDictionary<string, string> queryParameters)
    {
        var pay = new VnPayLibrary();
        foreach (var (key, value) in queryParameters)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                pay.AddResponseData(key, value);
            }
        }

        var vnp_orderId = pay.GetResponseData("vnp_TxnRef");
        var vnp_TransactionId = pay.GetResponseData("vnp_TransactionNo");
        var vnp_SecureHash = "";
        if (queryParameters.TryGetValue("vnp_SecureHash", out var hash))
        {
            vnp_SecureHash = hash;
        }
        var vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
        var vnp_OrderInfo = pay.GetResponseData("vnp_OrderInfo");

        bool checkSignature = pay.ValidateSignature(vnp_SecureHash, _configuration["VnPay:HashSecret"] ?? "");

        if (!checkSignature)
        {
            return new PaymentResponseModel
            {
                Success = false,
                VnPayResponseCode = "InvalidSignature"
            };
        }

        return new PaymentResponseModel
        {
            Success = true,
            PaymentMethod = "VnPay",
            OrderDescription = vnp_OrderInfo,
            OrderId = vnp_orderId,
            TransactionId = vnp_TransactionId,
            VnPayResponseCode = vnp_ResponseCode
        };
    }
}

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        StringBuilder data = new StringBuilder();
        foreach (KeyValuePair<string, string> kv in _requestData)
        {
            if (data.Length > 0)
            {
                data.Append('&');
            }
            data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value));
        }
        string queryString = data.ToString();
        string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, queryString);
        string paymentUrl = baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        return paymentUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseData();
        string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseData()
    {
        StringBuilder data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }
        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }
        foreach (KeyValuePair<string, string> kv in _responseData)
        {
            if (data.Length > 0)
            {
                data.Append('&');
            }
            data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value));
        }
        return data.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

public static class Utils
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }
}

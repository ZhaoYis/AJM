using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace AJM.Common
{
    /// <summary>
    /// Http����������
    /// </summary>
    public class HttpHelper
    {
        #region Ԥ���巽����
        //Ĭ�ϵı���
        private Encoding _encoding = Encoding.Default;
        //Post���ݱ���
        private Encoding _postencoding = Encoding.Default;
        //HttpWebRequest����������������
        private HttpWebRequest _request = null;
        //��ȡӰ���������ݶ���
        private HttpWebResponse _response = null;
        //���ñ��صĳ���ip�Ͷ˿�
        private IPEndPoint _ipEndPoint = null;
        #endregion

        #region Public
        /// <summary>
        /// �����ഫ������ݣ��õ���Ӧҳ������
        /// </summary>
        /// <param name="item">���������</param>
        /// <returns>����HttpResult����</returns>
        public HttpResult GetHtml(HttpItem item)
        {
            //���ز���
            HttpResult result = new HttpResult();
            try
            {
                //׼������
                SetRequest(item);
            }
            catch (Exception ex)
            {
                result.Cookie = string.Empty;
                result.Header = null;
                result.Html = ex.Message;
                result.StatusDescription = "���ò���ʱ����" + ex.Message;
                //���ò���ʱ����
                return result;
            }

            try
            {
                //��������
                using (_response = (HttpWebResponse)_request.GetResponse())
                {
                    GetData(item, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (_response = (HttpWebResponse)ex.Response)
                    {
                        GetData(item, result);
                    }
                }
                else
                {
                    result.Html = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }

            if (item.IsToLower)
                result.Html = result.Html.ToLower();
            return result;
        }
        #endregion

        #region GetData
        /// <summary>
        /// ��ȡ���ݵĲ������ķ���
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        private void GetData(HttpItem item, HttpResult result)
        {
            #region base
            //��ȡStatusCode
            result.StatusCode = _response.StatusCode;
            //��ȡStatusDescription
            result.StatusDescription = _response.StatusDescription;
            //��ȡ�����ʵ�URl
            result.ResponseUri = _response.ResponseUri.ToString();
            //��ȡHeaders
            result.Header = _response.Headers;
            //��ȡCookieCollection
            if (_response.Cookies != null) result.CookieCollection = _response.Cookies;
            //��ȡset-cookie
            if (_response.Headers["set-cookie"] != null) result.Cookie = _response.Headers["set-cookie"];
            #endregion

            #region byte
            //������ҳByte
            byte[] responseByte = GetByte();
            #endregion

            #region Html
            if (responseByte != null && responseByte.Length > 0)
            {
                //���ñ���
                SetEncoding(item, result, responseByte);
                //�õ����ص�HTML
                result.Html = _encoding.GetString(responseByte);
            }
            else
            {
                //û�з����κ�Html����
                result.Html = string.Empty;
            }
            #endregion
        }

        /// <summary>
        /// ���ñ���
        /// </summary>
        /// <param name="item">HttpItem</param>
        /// <param name="result">HttpResult</param>
        /// <param name="responseByte">byte[]</param>
        private void SetEncoding(HttpItem item, HttpResult result, byte[] responseByte)
        {
            //�Ƿ񷵻�Byte��������
            if (item.ResultType == ResultType.Byte) result.ResultByte = responseByte;
            //�����￪ʼ����Ҫ���ӱ�����
            if (_encoding == null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(responseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }

                if (c.Length > 2)
                {
                    try
                    {
                        _encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        _encoding = string.IsNullOrEmpty(_response.CharacterSet) ? Encoding.UTF8 : Encoding.GetEncoding(_response.CharacterSet);
                    }
                }
                else
                {
                    _encoding = string.IsNullOrEmpty(_response.CharacterSet) ? Encoding.UTF8 : Encoding.GetEncoding(_response.CharacterSet);
                }
            }
        }

        /// <summary>
        /// ��ȡ��ҳByte
        /// </summary>
        /// <returns></returns>
        private byte[] GetByte()
        {
            byte[] responseByte = null;
            MemoryStream stream;

            //GZIIP����
            if (_response.ContentEncoding != null && _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
            {
                //��ʼ��ȡ�������ñ��뷽ʽ
                stream = GetMemoryStream(new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress));
            }
            else
            {
                //��ʼ��ȡ�������ñ��뷽ʽ
                stream = GetMemoryStream(_response.GetResponseStream());
            }
            //��ȡByte
            responseByte = stream.ToArray();
            stream.Close();
            return responseByte;
        }

        /// <summary>
        /// 4.0����.net�汾ȡ����ʹ��
        /// </summary>
        /// <param name="streamResponse">��</param>
        private MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return stream;
        }
        #endregion

        #region SetRequest
        /// <summary>
        /// Ϊ����׼������
        /// </summary>
        ///<param name="item">�����б�</param>
        private void SetRequest(HttpItem item)
        {
            // ��֤֤��
            SetCer(item);

            if (item.IPEndPoint != null)
            {
                _ipEndPoint = item.IPEndPoint;
                //���ñ��صĳ���ip�Ͷ˿�
                _request.ServicePoint.BindIPEndPointDelegate = new BindIPEndPoint(BindIPEndPointCallback);
            }

            //����Header����
            if (item.Header != null && item.Header.Count > 0)
            {
                foreach (string key in item.Header.AllKeys)
                {
                    _request.Headers.Add(key, item.Header[key]);
                }
            }

            // ���ô���
            SetProxy(item);

            if (item.ProtocolVersion != null)
            {
                //��ȡ��������������� HTTP �汾
                _request.ProtocolVersion = item.ProtocolVersion;
            }
            //��ȡ������һ�� System.Boolean ֵ����ֵȷ���Ƿ�ʹ�� 100-Continue ��Ϊ
            _request.ServicePoint.Expect100Continue = item.Expect100Continue;
            //����ʽGet����Post
            _request.Method = item.Method;
            //����ʱʱ��
            _request.Timeout = item.Timeout;
            //�Ƿ�����������
            _request.KeepAlive = item.KeepAlive;
            //д�����ݳ�ʱʱ��
            _request.ReadWriteTimeout = item.ReadWriteTimeout;

            if (item.IfModifiedSince != null)
            {
                //��ȡ������ If-Modified-Since HTTP ��ͷ��ֵ
                _request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            }

            //Accept
            _request.Accept = item.Accept;
            //ContentType��������
            _request.ContentType = item.ContentType;
            //UserAgent�ͻ��˵ķ������ͣ�����������汾�Ͳ���ϵͳ��Ϣ
            _request.UserAgent = item.UserAgent;
            // ����
            _encoding = item.Encoding;
            //���ð�ȫƾ֤
            _request.Credentials = item.ICredentials;
            //����Cookie
            SetCookie(item);
            //��Դ��ַ
            _request.Referer = item.Referer;
            //�Ƿ�ִ����ת����
            _request.AllowAutoRedirect = item.Allowautoredirect;

            if (item.MaximumAutomaticRedirections > 0)
            {
                //��ȡ���������󽫸�����ض���������Ŀ
                _request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            }

            //����Post����
            SetPostData(item);

            //�����������
            if (item.Connectionlimit > 0)
            {
                //��ȡ�����ô� System.Net.ServicePoint ��������������������
                _request.ServicePoint.ConnectionLimit = item.Connectionlimit;
            }
        }

        /// <summary>
        /// ����֤��
        /// </summary>
        /// <param name="item"></param>
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.CerPath))
            {
                //��һ��һ��Ҫд�ڴ������ӵ�ǰ�档ʹ�ûص��ķ�������֤����֤��
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //��ʼ�����񣬲����������URL��ַ
                _request = (HttpWebRequest)WebRequest.Create(item.Url);
                SetCerList(item);
                //��֤����ӵ�������
                _request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //��ʼ�����񣬲����������URL��ַ
                _request = (HttpWebRequest)WebRequest.Create(item.Url);
                SetCerList(item);
            }
        }

        /// <summary>
        /// ���ö��֤��
        /// </summary>
        /// <param name="item"></param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    _request.ClientCertificates.Add(c);
                }
            }
        }

        /// <summary>
        /// ����Cookie
        /// </summary>
        /// <param name="item">Http����</param>
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie)) _request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //����CookieCollection
            if (item.ResultCookieType == ResultCookieType.CookieCollection)
            {
                _request.CookieContainer = new CookieContainer();
                if (item.CookieCollection != null && item.CookieCollection.Count > 0)
                    _request.CookieContainer.Add(item.CookieCollection);
            }
        }

        /// <summary>
        /// ����Post����
        /// </summary>
        /// <param name="item">Http����</param>
        private void SetPostData(HttpItem item)
        {
            //��֤�ڵõ����ʱ�Ƿ��д�������
            if (!_request.Method.Trim().ToLower().Contains("get"))
            {
                if (item.PostEncoding != null)
                {
                    _postencoding = item.PostEncoding;
                }

                byte[] buffer = null;
                //д��Byte����
                if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
                {
                    //��֤�ڵõ����ʱ�Ƿ��д�������
                    buffer = item.PostdataByte;
                }//д���ļ�
                else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrEmpty(item.Postdata))
                {
                    StreamReader r = new StreamReader(item.Postdata, _postencoding);
                    buffer = _postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //д���ַ���
                else if (!string.IsNullOrEmpty(item.Postdata))
                {
                    buffer = _postencoding.GetBytes(item.Postdata);
                }

                if (buffer != null)
                {
                    _request.ContentLength = buffer.Length;
                    _request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
                else
                {
                    _request.ContentLength = 0;
                }
            }
        }

        /// <summary>
        /// ���ô���
        /// </summary>
        /// <param name="item">��������</param>
        private void SetProxy(HttpItem item)
        {
            bool isIeProxy = false;
            if (!string.IsNullOrEmpty(item.ProxyIp))
            {
                isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrEmpty(item.ProxyIp) && !isIeProxy)
            {
                //���ô��������
                if (item.ProxyIp.Contains(":"))
                {
                    string[] plist = item.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()))
                    {
                        //��������
                        Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd)
                    };
                    //����ǰ�������
                    _request.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(item.ProxyIp, false)
                    {
                        //��������
                        Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd)
                    };
                    //����ǰ�������
                    _request.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
                //����ΪIE����
            }
            else
            {
                _request.Proxy = item.WebProxy;
            }
        }
        #endregion

        #region private main
        /// <summary>
        /// �ص���֤֤������
        /// </summary>
        /// <param name="sender">������</param>
        /// <param name="certificate">֤��</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

        /// <summary>
        /// ͨ������������ԣ������ڷ������ӵ�ʱ��󶨿ͻ��˷���������ʹ�õ�IP��ַ�� 
        /// </summary>
        /// <param name="servicePoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        private IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            return _ipEndPoint;//�˿ں�
        }
        #endregion
    }

    #region �������

    /// <summary>
    /// Http����ο���
    /// </summary>
    public class HttpItem
    {
        private string _url = string.Empty;
        /// <summary>
        /// ����URL������д
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        private string _method = "GET";
        /// <summary>
        /// ����ʽĬ��ΪGET��ʽ,��ΪPOST��ʽʱ��������Postdata��ֵ
        /// </summary>
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        private int _timeout = 90 * 1000;
        /// <summary>
        /// Ĭ������ʱʱ�䣬Ĭ��90��
        /// </summary>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private int _readWriteTimeout = 60 * 1000;
        /// <summary>
        /// Ĭ��д��Post���ݳ�ʱ�䣬Ĭ��60��
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return _readWriteTimeout; }
            set { _readWriteTimeout = value; }
        }

        private Boolean _keepAlive = true;
        /// <summary>
        ///  ��ȡ������һ��ֵ����ֵָʾ�Ƿ��� Internet ��Դ�����־�������Ĭ��Ϊtrue��
        /// </summary>
        public Boolean KeepAlive
        {
            get { return _keepAlive; }
            set { _keepAlive = value; }
        }

        private string _accept = "text/html, application/xhtml+xml, */*";
        /// <summary>
        /// �����ͷֵ��Ĭ��Ϊtext/html, application/xhtml+xml, */*
        /// <para>1��application/json</para>
        /// </summary>
        public string Accept
        {
            get { return _accept; }
            set { _accept = value; }
        }

        private string _contentType = "text/html";
        /// <summary>
        /// ���󷵻����ͣ�Ĭ��Ϊtext/html
        /// <para>1��application/x-www-form-urlencoded������� POST �ύ���ݵķ�ʽ</para>
        /// <para>2��multipart/form-data����Ҫ�����ϴ��ļ�</para>
        /// <para>3��application/json���������Ϣ���������л���� JSON �ַ���</para>
        /// <para>4��text/xml</para>
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        private string _userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// �ͻ��˷�����ϢĬ��Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// �����MSIE 8.0����IE8, Windows NT 6.1 ��Ӧ����ϵͳ windows 7 
        /// <para>1��IE�����汾���͵�UserAgent���£�</para> 
        /// <para>Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0)</para> 
        /// <para>Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2)</para> 
        /// <para>Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)</para> 
        /// <para>Mozilla/4.0 (compatible; MSIE 5.0; Windows NT)</para> 
        /// <para>2��Firefox��UserAgent���£� </para> 
        /// <para>Mozilla/5.0 (Windows; U; Windows NT 5.2) Gecko/2008070208 Firefox/3.0.1</para> 
        /// <para>Mozilla/5.0 (Windows; U; Windows NT 5.1) Gecko/20070309 Firefox/2.0.0.3</para> 
        /// <para>Mozilla/5.0 (Windows; U; Windows NT 5.1) Gecko/20070803 Firefox/1.5.0.12</para> 
        /// <para>�����N: ��ʾ�ް�ȫ���� I: ��ʾ����ȫ���� U: ��ʾǿ��ȫ����</para> 
        /// <para>3��Opera���͵�UserAgent���£�</para> 
        /// <para>Opera/9.27 (Windows NT 5.2; U; zh-cn)</para> 
        /// <para>Opera/8.0 (Macintosh; PPC Mac OS X; U; en)</para> 
        /// <para>Mozilla/5.0 (Macintosh; PPC Mac OS X; U; en) Opera 8.0 </para> 
        /// <para>4��Safari���͵�UserAgent���£�</para> 
        /// <para>Mozilla/5.0 (Windows; U; Windows NT 5.2) AppleWebKit/525.13 (KHTML, like Gecko) Version/3.1 Safari/525.13</para> 
        ///  <para>Mozilla/5.0 (iPhone; U; CPU like Mac OS X) AppleWebKit/420.1 (KHTML, like Gecko) Version/3.0 Mobile/4A93 Safari/419.3</para> 
        /// <para>5��Chrome��UserAgent���£�</para> 
        /// <para>Mozilla/5.0 (Windows; U; Windows NT 5.2) AppleWebKit/525.13 (KHTML, like Gecko) Chrome/0.2.149.27 Safari/525.13 </para> 
        /// <para>6��Navigator��UserAgent���£�</para> 
        /// <para>Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1.12) Gecko/20080219 Firefox/2.0.0.12 Navigator/9.0.0.6</para> 
        /// <para>7����׿ ԭ�������</para>
        /// <para>Mozilla/5.0 (Linux; U; Android 4.0.3; zh-cn; M032 Build/IML74K) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30</para>
        /// <para>8��iPhone Safria</para>
        /// <para>Mozilla/5.0 (iPhone; CPU iPhone OS 5_1_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B206 Safari/7534.48.3</para>
        /// <para>9������ �Դ������</para>
        /// <para>Nokia5320/04.13 (SymbianOS/9.3; U; Series60/3.2 Mozilla/5.0; Profile/MIDP-2.1 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML, like Gecko) Safari/413</para>
        /// </summary>
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        private Encoding _encoding = null;
        /// <summary>
        /// �������ݱ���Ĭ��ΪNUll,�����Զ�ʶ��,һ��Ϊutf-8,gbk,gb2312
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        private PostDataType _postDataType = PostDataType.String;
        /// <summary>
        /// Post����������
        /// </summary>
        public PostDataType PostDataType
        {
            get { return _postDataType; }
            set { _postDataType = value; }
        }

        private string _postdata = string.Empty;
        /// <summary>
        /// Post����ʱҪ���͵��ַ���Post����
        /// </summary>
        public string Postdata
        {
            get { return _postdata; }
            set { _postdata = value; }
        }

        private byte[] _postdataByte = null;
        /// <summary>
        /// Post����ʱҪ���͵�Byte���͵�Post����
        /// </summary>
        public byte[] PostdataByte
        {
            get { return _postdataByte; }
            set { _postdataByte = value; }
        }

        private WebProxy _webProxy;
        /// <summary>
        /// ���ô�����󣬲���ʹ��IEĬ�����þ�����ΪNull�����Ҳ�Ҫ����ProxyIp
        /// </summary>
        public WebProxy WebProxy
        {
            get { return _webProxy; }
            set { _webProxy = value; }
        }

        private CookieCollection _cookiecollection = null;
        /// <summary>
        /// Cookie���󼯺�
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return _cookiecollection; }
            set { _cookiecollection = value; }
        }

        private string _cookie = string.Empty;
        /// <summary>
        /// ����ʱ��Cookie
        /// </summary>
        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        private string _referer = string.Empty;
        /// <summary>
        /// ��Դ��ַ���ϴη��ʵ�ַ
        /// </summary>
        public string Referer
        {
            get { return _referer; }
            set { _referer = value; }
        }

        private string _cerPath = string.Empty;
        /// <summary>
        /// ֤�����·��
        /// </summary>
        public string CerPath
        {
            get { return _cerPath; }
            set { _cerPath = value; }
        }

        private Boolean _isToLower = false;
        /// <summary>
        /// �Ƿ�����Ϊȫ��Сд��Ĭ��Ϊ��ת��
        /// </summary>
        public Boolean IsToLower
        {
            get { return _isToLower; }
            set { _isToLower = value; }
        }

        private Boolean _allowautoredirect = false;
        /// <summary>
        /// ֧����תҳ�棬��ѯ���������ת���ҳ�棬Ĭ���ǲ���ת
        /// </summary>
        public Boolean Allowautoredirect
        {
            get { return _allowautoredirect; }
            set { _allowautoredirect = value; }
        }

        private int _connectionlimit = 1024;
        /// <summary>
        /// ���������
        /// </summary>
        public int Connectionlimit
        {
            get { return _connectionlimit; }
            set { _connectionlimit = value; }
        }

        private string _proxyusername = string.Empty;
        /// <summary>
        /// ����Proxy �������û���
        /// </summary>
        public string ProxyUserName
        {
            get { return _proxyusername; }
            set { _proxyusername = value; }
        }

        private string _proxypwd = string.Empty;
        /// <summary>
        /// ���� ����������
        /// </summary>
        public string ProxyPwd
        {
            get { return _proxypwd; }
            set { _proxypwd = value; }
        }

        private string _proxyip = string.Empty;
        /// <summary>
        /// ���� ����IP ,���Ҫʹ��IE���������Ϊieproxy
        /// </summary>
        public string ProxyIp
        {
            get { return _proxyip; }
            set { _proxyip = value; }
        }

        private ResultType _resulttype = ResultType.String;
        /// <summary>
        /// ���÷�������String��Byte
        /// </summary>
        public ResultType ResultType
        {
            get { return _resulttype; }
            set { _resulttype = value; }
        }

        private WebHeaderCollection _header = new WebHeaderCollection();
        /// <summary>
        /// header����
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return _header; }
            set { _header = value; }
        }

        private Version _protocolVersion = System.Net.HttpVersion.Version11;
        /// <summary>
        //  ��ȡ��������������� HTTP �汾�����ؽ��:��������� HTTP �汾��Ĭ��Ϊ System.Net.HttpVersion.Version11��
        /// </summary>
        public Version ProtocolVersion
        {
            get { return _protocolVersion; }
            set { _protocolVersion = value; }
        }

        private Boolean _expect100Continue = false;
        /// <summary>
        ///  ��ȡ������һ�� System.Boolean ֵ����ֵȷ���Ƿ�ʹ�� 100-Continue ��Ϊ����� POST ������Ҫ 100-Continue ��Ӧ����Ϊ true������Ϊ false��Ĭ��ֵΪ true��
        /// </summary>
        public Boolean Expect100Continue
        {
            get { return _expect100Continue; }
            set { _expect100Continue = value; }
        }

        private X509CertificateCollection _clentCertificates;
        /// <summary>
        /// ����509֤�鼯��
        /// </summary>
        public X509CertificateCollection ClentCertificates
        {
            get { return _clentCertificates; }
            set { _clentCertificates = value; }
        }

        private Encoding _postEncoding = Encoding.Default;
        /// <summary>
        /// ���û��ȡPost��������,Ĭ�ϵ�ΪDefault����
        /// </summary>
        public Encoding PostEncoding
        {
            get { return _postEncoding; }
            set { _postEncoding = value; }
        }

        private ResultCookieType _resultCookieType = ResultCookieType.String;
        /// <summary>
        /// Cookie��������,Ĭ�ϵ���ֻ�����ַ�������
        /// </summary>
        public ResultCookieType ResultCookieType
        {
            get { return _resultCookieType; }
            set { _resultCookieType = value; }
        }

        private ICredentials _iCredentials = CredentialCache.DefaultCredentials;
        /// <summary>
        /// ��ȡ����������������֤��Ϣ��
        /// </summary>
        public ICredentials ICredentials
        {
            get { return _iCredentials; }
            set { _iCredentials = value; }
        }

        /// <summary>
        /// �������󽫸�����ض���������Ŀ
        /// </summary>
        private int _maximumAutomaticRedirections;

        public int MaximumAutomaticRedirections
        {
            get { return _maximumAutomaticRedirections; }
            set { _maximumAutomaticRedirections = value; }
        }

        private DateTime? _ifModifiedSince = null;
        /// <summary>
        /// ��ȡ������IfModifiedSince��Ĭ��Ϊ��ǰ���ں�ʱ��
        /// </summary>
        public DateTime? IfModifiedSince
        {
            get { return _ifModifiedSince; }
            set { _ifModifiedSince = value; }
        }

        #region ip-port
        private IPEndPoint _ipEndPoint = null;
        /// <summary>
        /// ���ñ��صĳ���ip�Ͷ˿�
        /// </summary>]
        /// <example>
        ///item.IPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"),80);
        /// </example>
        public IPEndPoint IPEndPoint
        {
            get { return _ipEndPoint; }
            set { _ipEndPoint = value; }
        }
        #endregion
    }

    #endregion

    #region ���ز���

    /// <summary>
    /// Http���ز�����
    /// </summary>
    public class HttpResult
    {
        private string _cookie;
        /// <summary>
        /// Http���󷵻ص�Cookie
        /// </summary>
        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        private CookieCollection _cookieCollection;
        /// <summary>
        /// Cookie���󼯺�
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return _cookieCollection; }
            set { _cookieCollection = value; }
        }

        private string _html = string.Empty;
        /// <summary>
        /// ���ص�String�������� ֻ��ResultType.Stringʱ�ŷ������ݣ��������Ϊ��
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        private byte[] _resultByte;
        /// <summary>
        /// ���ص�Byte���� ֻ��ResultType.Byteʱ�ŷ������ݣ��������Ϊ��
        /// </summary>
        public byte[] ResultByte
        {
            get { return _resultByte; }
            set { _resultByte = value; }
        }

        private WebHeaderCollection _header;
        /// <summary>
        /// header����
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return _header; }
            set { _header = value; }
        }

        private string _statusDescription;
        /// <summary>
        /// ����״̬˵��
        /// </summary>
        public string StatusDescription
        {
            get { return _statusDescription; }
            set { _statusDescription = value; }
        }

        private HttpStatusCode _statusCode;
        /// <summary>
        /// ����״̬��,Ĭ��ΪOK
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        /// <summary>
        /// �����ʵ�URl
        /// </summary>
        public string ResponseUri { get; set; }

        /// <summary>
        /// ��ȡ�ض����URl
        /// </summary>
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header != null && Header.Count > 0)
                    {
                        string baseurl = Header["location"].ToString().Trim();
                        string locationurl = baseurl.ToLower();
                        if (!string.IsNullOrWhiteSpace(locationurl))
                        {
                            bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                            if (!b)
                            {
                                baseurl = new Uri(new Uri(ResponseUri), baseurl).AbsoluteUri;
                            }
                        }
                        return baseurl;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
    }

    #endregion

    #region ö��

    /// <summary>
    /// ��������
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// ��ʾֻ�����ַ��� ֻ��Html������
        /// </summary>
        String,
        /// <summary>
        /// ��ʾ�����ַ������ֽ��� ResultByte��Html�������ݷ���
        /// </summary>
        Byte
    }

    /// <summary>
    /// Post�����ݸ�ʽĬ��Ϊstring
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// �ַ������ͣ���ʱ����Encoding�ɲ�����
        /// </summary>
        String,
        /// <summary>
        /// Byte���ͣ���Ҫ����PostdataByte������ֵ����Encoding������Ϊ��
        /// </summary>
        Byte,
        /// <summary>
        /// ���ļ���Postdata��������Ϊ�ļ��ľ���·������������Encoding��ֵ
        /// </summary>
        FilePath
    }

    /// <summary>
    /// Cookie��������
    /// </summary>
    public enum ResultCookieType
    {
        /// <summary>
        /// ֻ�����ַ������͵�Cookie
        /// </summary>
        String,
        /// <summary>
        /// CookieCollection��ʽ��Cookie����ͬʱҲ����String���͵�cookie
        /// </summary>
        CookieCollection
    } 
    #endregion
}
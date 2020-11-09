namespace MarkdownMonster.Services
{
    /// <summary>
    /// Result sent back from the Web request to the server.
    /// Result is serialized into JSON so the result value should be
    /// a serializable value (using JSON.NET so object/anonymous types work)
    /// </summary>
    public class WebServerResult
    {

        /// <summary>
        /// Use this ctor to return a non-data result
        /// </summary>
        public WebServerResult()
        {
            hasNoData = true;
            HttpStatusCode = 204;
        }

        /// <summary>
        /// Use this ctor to return a result with data
        /// </summary>
        /// <param name="result"></param>
        public WebServerResult(object result)
        {
            Result = result;
        }

        /// <summary>
        /// Use this ctor to return an error
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="httpStatusCode"></param>
        public WebServerResult(string errorMessage, int httpStatusCode)
        {
            if (httpStatusCode == 0)
                httpStatusCode = 500;

            ErrorMessage = errorMessage;
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Error flag that can be checked for errors
        /// </summary>
        public bool IsError {get; set; }

        /// <summary>
        /// Set to true if returning no data
        /// </summary>
        public bool hasNoData {get; set; }

        /// <summary>
        /// The result to return to the client on a success response
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// An error message when an error occurs
        /// </summary>
        public string ErrorMessage {get; set; }

        /// <summary>
        /// Optional HTTP status code - defaults: 200 for success, 500 for errors
        /// </summary>
        public int HttpStatusCode { get; set; }= 200;

        /// <summary>
        /// The requests content type. Typically will be JSON object unless no
        /// data is returned.
        /// </summary>
        public string ContentType { get; set; } = "application/json";
    }
}

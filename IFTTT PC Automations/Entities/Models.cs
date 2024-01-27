using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IFTTT_PC_Automations.Entities
{
    public class Event
    {
        #region Properties
        public bool Enabled { get; set; }
        public EventType EventType { get; set; }
        public string Value { get; set; }
        public Dictionary<string, Action> Actions { get; set; } = new Dictionary<string, Action>();
        #endregion

        #region Constructors
        public Event(bool enabled, EventType eventType, string value)
        {
            Enabled = enabled;
            EventType = eventType;
            Value = value;
        }
        #endregion
    }

    public class Action
    {
        #region Properties
        public bool Enabled { get; set; }
        public string AppletEventName { get; set; }
        public string? JSONPayload { get; set; }
        #endregion

        #region Constructors
        public Action() { }

        public Action(string appletEventName)
        {
            AppletEventName = appletEventName;
        }

        public Action(string appletEventName, string? jsonPayload)
        {
            AppletEventName = appletEventName;
            JSONPayload = jsonPayload;
        }

        public Action(bool enabled, string appletEventName, string? jsonPayload)
        {
            Enabled = enabled;
            AppletEventName = appletEventName;
            JSONPayload = jsonPayload;
        }
        #endregion
    }

    public class DynamicData
    {
        #region Properties
        public string? Name { get; set; }
        public dynamic? Data { get; set; }
        #endregion

        #region Constructors
        public DynamicData() { }

        public DynamicData(string? name, dynamic? data)
        {
            Name = name;
            Data = data;
        }
        #endregion
    }

    public class RequestResponse //: FunctionResponse
    {
        #region Properties
        public string AppletName { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string? Body { get; set; }
        public bool Error { get; set; }
        #endregion

        #region Constructors
        public RequestResponse() { }

        public RequestResponse(bool error)
        {
            Error = error;
        }

        public RequestResponse(string? body, bool error)
        {
            Body = body;
            Error = error;
        }

        public RequestResponse(string appletName, HttpStatusCode statusCode, string? body, bool error)
        {
            AppletName = appletName;
            StatusCode = statusCode;
            Body = body;
            Error = error;
        }
        #endregion
    }

    public class FunctionResponse
    {
        #region Properties
        public bool Error { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        #endregion

        #region Constructors
        public FunctionResponse() { }

        public FunctionResponse(bool error)
        {
            Error = error;
        }

        public FunctionResponse(bool error, string message)
        {
            Error = error;
            Message = message;
        }

        public FunctionResponse(bool error, string message, string stackTrace)
        {
            Error = error;
            Message = message;
            StackTrace = stackTrace;
        }

        public FunctionResponse(Exception exception)
        {
            Error = true;
            Message = exception.Message;
            StackTrace = exception.StackTrace;
        }
        #endregion

        #region Methods
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.None);
        #endregion
    }

    public class Validation
    {
        #region Properties
        public bool Valid { get; set; }
        public string? Message { get; set; }
        #endregion

        #region Constructors
        public Validation(bool valid)
        {
            Valid = valid;
        }

        public Validation(bool valid, string message)
        {
            Valid = valid;
            Message = message;
        }
        #endregion
    }

    public class Log
    {
        #region Properties
        public DateTime DateTime { get; } = DateTime.Now;
        public string EventName { get; set; }
        public Dictionary<string, RequestResponse> ActionsResults { get; set; }
        #endregion

        #region Constructors
        public Log(string eventName, Dictionary<string, RequestResponse> actionsResults)
        {
            EventName = eventName;
            ActionsResults = actionsResults;
        }
        #endregion

        #region Methods
        public override string ToString() => ToJsonString();
        public string ToJsonString(Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(this, formatting);
        #endregion
    }

    public class UnsavedLog
    {
        #region Properties
        public Log Log { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        #endregion

        #region Constructors
        public UnsavedLog(Log log, string message, string? stackTrace)
        {

            Log = log;
            Message = message;
            StackTrace = stackTrace;
        }
        #endregion
    }

    public class AppError
    {
        #region Properties
        public DateTime DateTime { get; } = DateTime.Now;
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public MethodInfo? MethodInfo { get; set; }
        #endregion

        #region Constructors
        public AppError(Exception exception, MethodInfo methodInfo)
        {
            Message = exception.Message;
            StackTrace = exception.StackTrace;
            MethodInfo = methodInfo;
        }

        public AppError(string message, MethodInfo methodInfo)
        {
            Message = message;
            MethodInfo = methodInfo;
        }
        #endregion

        #region Methods
        public override string ToString() => ToJsonString();
        public string ToJsonString(Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(this, formatting);
        #endregion
    }

    public class MethodInfo
    {
        #region Properties
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        #endregion

        #region Constructors
        public MethodInfo(string name, string filePath, int lineNumber)
        {
            Name = name;
            FilePath = filePath;
            LineNumber = lineNumber;
        }
        #endregion
    }

    public class Statistics
    {
        #region Properties
        public int EventsFired { get; set; } = 0;
        public int ActionsFired { get; set; } = 0;
        public int ActionsFailed { get; set; } = 0;
        #endregion

        #region Constructors
        public Statistics() { }

        public Statistics(int eventsFired, int actionsFired, int actionsFailed)
        {
            EventsFired = eventsFired;
            ActionsFired = actionsFired;
            ActionsFailed = actionsFailed;
        }
        #endregion
    }

    public class IftttErrorResponse
    {
        #region Properties
        [JsonProperty("errors")]
        public List<IftttError> Errors { get; set; }
        #endregion
    }

    public class IftttError
    {
        #region Properties
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
        #endregion
    }

    public class TextEditor
    {
        #region Properties
        public string Name { get; set; }
        public bool Exists { get; set; }
        public string? Path { get; set; }
        #endregion

        #region Constructors
        public TextEditor(string name, bool exists)
        {
            Name = name;
            Exists = exists;
        }

        public TextEditor(string name, bool exists, string path)
        {
            Name = name;
            Exists = exists;
            Path = path;
        }
        #endregion
    }

    public class ComboboxItem
    {
        #region Properties
        public string Text { get; set; }
        public string Value { get; set; }
        #endregion

        #region Constructors
        public ComboboxItem(string text, string value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
        #endregion
    }
}

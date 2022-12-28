using Azure.Communication.CallingServer;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using AdaniCall.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CallRecord;
using System.Configuration;
using AdaniCall.Utility.Common;
using AdaniCall.Business.BusinessFacade;
using AdaniCall.Business.DataAccess.Constants;

namespace AdaniCall.Controllers.API
{
    [RoutePrefix("api/RecordingAudio")]
    public class RecordingAudioController : ApiController
    {
        private readonly string blobStorageConnectionString;
        private readonly string callbackUri;
        private readonly string containerName;
        private readonly CallingServerClient callingServerClient;
        private const string CallRecodingActiveErrorCode = "8553";
        private const string CallRecodingActiveError = "Recording is already in progress, one recording can be active at one time.";
        private readonly string _module = "AdaniCall.Controllers.API.RecordingAudioController";

        //private string RootPath = ConfigurationManager.AppSettings["RootPath"].ToString() + @"\AllFiles\Audio";
        private string RootPath = ConfigurationManager.AppSettings["RootPath"].ToString() + ConfigurationManager.AppSettings["AudioPath"].ToString();
        //public ILogger<RecordingController> Logger { get; set; }
        static Dictionary<string, string> recordingData = new Dictionary<string, string>();
        public static string recFileFormat;

        public RecordingAudioController()
        {
            blobStorageConnectionString = ConfigurationManager.AppSettings["BlobStorageConnectionString"].ToString();
            callbackUri = ConfigurationManager.AppSettings["CallBackURIAudio"].ToString();
            containerName = ConfigurationManager.AppSettings["ContainerName"].ToString();
            callingServerClient = new CallingServerClient(ConfigurationManager.AppSettings["CallingServerClient"].ToString());
        }

        /// <summary>
        /// Method to start call recording
        /// </summary>
        /// <param name="serverCallId">Conversation id of the call</param>
        [HttpPost]
        [Route("startRecording")]
        //public async Task<IHttpActionResult> StartRecordingAsync(string serverCallId)
        public async Task<IHttpActionResult> StartRecordingAsync(RecordParams objRecordParams)
        {
            try
            {
                string serverCallId = objRecordParams.sCallID.ToString();
                string recordingFormat = objRecordParams.recordingFormat.ToString();
                string uniqueCallID = objRecordParams.UniqueCallID.ToString();
                if (!string.IsNullOrEmpty(serverCallId))
                {
                    var uri = new Uri(callbackUri);
                    //Passing RecordingContent initiates recording in specific format. audio/audiovideo
                    //RecordingChannel is used to pass the channel type. mixed/unmixed
                    //RecordingFormat is used to pass the format of the recording. mp4/mp3/wav
                    //var startRecordingResponse =
                    //    recordingFormat == "wav" 
                    //    ? await callingServerClient.InitializeServerCall(serverCallId).StartRecordingAsync(uri, RecordingContent.Audio, RecordingChannel.Unmixed, RecordingFormat.Wav).ConfigureAwait(false) 
                    //    : await callingServerClient.InitializeServerCall(serverCallId).StartRecordingAsync(uri).ConfigureAwait(false);

                    var startRecordingResponse = await callingServerClient.InitializeServerCall(serverCallId).StartRecordingAsync(uri, RecordingContent.Audio, RecordingChannel.Unmixed, RecordingFormat.Wav).ConfigureAwait(false);

                    //Logger.LogInformation($"StartRecordingAsync response -- >  {startRecordingResponse.GetRawResponse()}, Recording Id: {startRecordingResponse.Value.RecordingId}");

                    var recordingId = startRecordingResponse.Value.RecordingId;
                    if (!recordingData.ContainsKey(serverCallId))
                    {
                        recordingData.Add(serverCallId, string.Empty);
                    }
                    recordingData[serverCallId] = recordingId;

                    if (uniqueCallID != "")
                    {
                        CallTransactionsBusinessFacade objBF = new CallTransactionsBusinessFacade();
                        objBF.UpdateCallTransactions(CallTransactionsDBFields.ServerCallAudioID + "='" + serverCallId + "'," + CallTransactionsDBFields.RecordingAudioID + "='" + recordingId + "'", CallTransactionsDBFields.UniqueCallID + "='" + uniqueCallID + "'");
                    }
                    return Json(recordingId);
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new { Message = "serverCallId is invalid" });
                    //return BadRequest(new { Message = "serverCallId is invalid" });
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "StartRecordingAsync(sCallID=" + objRecordParams.sCallID + ",recordingFormat:" + objRecordParams.recordingFormat + ")", ex.Source, ex.Message);
                if (ex.Message.Contains(CallRecodingActiveErrorCode))
                {
                    Log.WriteLog(_module, "StartRecordingAsync(Message=" + CallRecodingActiveError + ")", ex.Source, ex.Message);
                    return Content(HttpStatusCode.BadRequest, new { Message = CallRecodingActiveError });
                    //return BadRequest(new { Message = CallRecodingActiveError });
                }
                return Json(new { Exception = ex });
            }
        }

        /// <summary>
        /// Web hook to receive the recording file update status event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getRecordingFile")]
        public async Task<IHttpActionResult> GetRecordingFile([FromBody] object request)
        {
            try
            {
                //string output = JsonConvert.SerializeObject(request);
                var httpContent = new BinaryData(request.ToString()).ToStream();
                //var httpContent = new BinaryData(output.ToString()).ToStream();
                EventGridEvent cloudEvent = EventGridEvent.ParseMany(BinaryData.FromStream(httpContent)).FirstOrDefault();

                if (cloudEvent.EventType == SystemEventNames.EventGridSubscriptionValidation)
                {
                    var eventData = cloudEvent.Data.ToObjectFromJson<SubscriptionValidationEventData>();

                    //Logger.LogInformation("Microsoft.EventGrid.SubscriptionValidationEvent response  -- >" + cloudEvent.Data);

                    var responseData = new SubscriptionValidationResponse
                    {
                        ValidationResponse = eventData.ValidationCode
                    };

                    if (responseData.ValidationResponse != null)
                    {
                        return Json(responseData);
                    }
                }

                if (cloudEvent.EventType == SystemEventNames.AcsRecordingFileStatusUpdated)
                {
                    //Logger.LogInformation($"Event type is -- > {cloudEvent.EventType}");

                    //Logger.LogInformation("Microsoft.Communication.RecordingFileStatusUpdated response  -- >" + cloudEvent.Data);

                    var eventData = cloudEvent.Data.ToObjectFromJson<AcsRecordingFileStatusUpdatedEventData>();

                    //Logger.LogInformation("Start processing metadata -- >");

                    await ProcessFile(eventData.RecordingStorageInfo.RecordingChunks[0].MetadataLocation,
                        eventData.RecordingStorageInfo.RecordingChunks[0].DocumentId,
                        FileFormat.Json,
                        FileDownloadType.Metadata);

                    //Logger.LogInformation("Start processing recorded media -- >");

                    await ProcessFile(eventData.RecordingStorageInfo.RecordingChunks[0].ContentLocation,
                        eventData.RecordingStorageInfo.RecordingChunks[0].DocumentId,
                        string.IsNullOrWhiteSpace(recFileFormat) ? FileFormat.Wav : recFileFormat,
                        FileDownloadType.Recording);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "GetRecordingFile()", ex.Source, ex.Message);
                return Json(new { Exception = ex });
            }
        }

        private async Task<bool> ProcessFile(string downloadLocation, string documentId, string fileFormat, string downloadType)
        {
            try
            {
                var recordingDownloadUri = new Uri(downloadLocation);
                var response = callingServerClient.DownloadStreamingAsync(recordingDownloadUri);

                //Logger.LogInformation($"Download {downloadType} response  -- >" + response.Result.GetRawResponse());
                //Logger.LogInformation($"Save downloaded {downloadType} -- >");

                string filePath = RootPath + @"\" + documentId + "." + fileFormat;
                using (Stream streamToReadFrom = response.Result.Value)
                {
                    using (Stream streamToWriteTo = System.IO.File.Open(filePath, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        await streamToWriteTo.FlushAsync();
                    }
                }

                string recordedfileName = "";
                string uniqueCallID = "";
                if (string.Equals(downloadType, FileDownloadType.Metadata, StringComparison.InvariantCultureIgnoreCase) && System.IO.File.Exists(filePath))
                {
                    Root deserializedFilePath = JsonConvert.DeserializeObject<Root>(System.IO.File.ReadAllText(filePath));
                    recFileFormat = deserializedFilePath.recordingInfo.format;
                    recordedfileName = deserializedFilePath.chunkDocumentId;
                    uniqueCallID = deserializedFilePath.callId;

                    //Logger.LogInformation($"Recording File Format is -- > {recFileFormat}");
                }

                //Logger.LogInformation($"Starting to upload {downloadType} to BlobStorage into container -- > {containerName}");

                //Upload on Cloud storage
                var blobStorageHelperInfo = await BlobStorageHelper.UploadFileAsync(blobStorageConnectionString, containerName, filePath, filePath);
                if (blobStorageHelperInfo.Status)
                {
                    //Logger.LogInformation(blobStorageHelperInfo.Message);
                    //Logger.LogInformation($"Deleting temporary {downloadType} file being created");

                    System.IO.File.Delete(filePath);
                }
                else
                {
                    //Logger.LogError($"{downloadType} file was not uploaded,{blobStorageHelperInfo.Message}");
                }

                CallTransactionsBusinessFacade objBF = new CallTransactionsBusinessFacade();
                if (recFileFormat.ToLower() == "wav")
                {
                    objBF.UpdateCallTransactions(CallTransactionsDBFields.AudioFileName + "='" + recordedfileName + "'," + CallTransactionsDBFields.UpdatedDate + "=getDate()", CallTransactionsDBFields.UniqueCallID + "='" + uniqueCallID + "'");
                }
                //else if (recFileFormat.ToLower() == "mp4")
                //{
                //    objBF.UpdateCallTransactions(CallTransactionsDBFields.VideoFileName + "='" + recordedfileName + "'," + CallTransactionsDBFields.UpdatedDate + "=getDate()", CallTransactionsDBFields.UniqueCallID + "='" + uniqueCallID + "'");
                //}

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "GetRecordingFile()", ex.Source, ex.Message);
                return false;
            }
        }


        //********** Replace above API with this if you want to start recording with additional arguments. *************

        //public RecordingController(ILogger<RecordingController> logger)
        //{
        //    blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=csg10032001954ffabf;AccountKey=0qCy9JCL9WLksxA2KLAuFDVY6M7tdf1Jj/stJTw+o2+ttkqjFVgOh9k7QjsZeriwoxsayaSKp2dfwqWauvBgUg==;EndpointSuffix=core.windows.net";
        //    callbackUri = "https://localhost:44390/Home/CallBack";
        //    containerName = "recordingcontainer";
        //    callingServerClient = new CallingServerClient("endpoint=https://teamscall.communication.azure.com/;accesskey=knRaFOQ4cK4Uz5bLdUR0UdwKwYl/vBOijFGuO2s+944xNFtUbSBwvr8bB7LPJcu8D4gnG+igN3Z+KEwNZByyNw==");
        //}

        ///// <summary>
        ///// Method to start call recording using given parameters
        ///// </summary>
        ///// <param name="serverCallId">Conversation id of the call</param>
        ///// <param name="recordingContent">Recording content type. audiovideo/audio</param>
        ///// <param name="recordingChannel">Recording channel type. mixed/unmixed</param>
        ///// <param name="recordingFormat">Recording format type. mp3/mp4/wav</param>
        ////[HttpGet]
        ////[Route("startRecording")]
        //public async Task<IHttpActionResult> StartRecordingAsync(string serverCallId, string recordingContent, string recordingChannel, string recordingFormat)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(serverCallId))
        //        {
        //            RecordingContent recContentVal;
        //            RecordingChannel recChannelVal;
        //            RecordingFormat recFormatVal;

        //            //Passing RecordingContent initiates recording in specific format. audio/audiovideo
        //            //RecordingChannel is used to pass the channel type. mixed/unmixed
        //            //RecordingFormat is used to pass the format of the recording. mp4/mp3/wav
        //            var startRecordingResponse = await callingServerClient
        //                .InitializeServerCall(serverCallId)
        //                .StartRecordingAsync(
        //                    new Uri(callbackUri),
        //                    Mapper.RecordingContentMap.TryGetValue(recordingContent, out recContentVal) ? recContentVal : RecordingContent.AudioVideo,
        //                    Mapper.RecordingChannelMap.TryGetValue(recordingChannel, out recChannelVal) ? recChannelVal : RecordingChannel.Mixed,
        //                    Mapper.RecordingFormatMap.TryGetValue(recordingFormat, out recFormatVal) ? recFormatVal : RecordingFormat.Mp4
        //                ).ConfigureAwait(false);

        //            //Logger.LogInformation($"StartRecordingAudioAsync response -- >  {startRecordingResponse.GetRawResponse()}, Recording Id: {startRecordingResponse.Value.RecordingId}");

        //            var recordingId = startRecordingResponse.Value.RecordingId;
        //            if (!recordingData.ContainsKey(serverCallId))
        //            {
        //                recordingData.Add(serverCallId, string.Empty);
        //            }
        //            recordingData[serverCallId] = recordingId;

        //            return Json(recordingId);
        //        }
        //        else
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Message = "serverCallId is invalid" });
        //            //return BadRequest(new { Message = "serverCallId is invalid" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains(CallRecodingActiveErrorCode))
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Message = CallRecodingActiveError });
        //            //return BadRequest(new { Message = CallRecodingActiveError });
        //        }
        //        return Json(new { Exception = ex });
        //    }
        //}

        ///// <summary>
        ///// Method to pause call recording
        ///// </summary>
        ///// <param name="serverCallId">Conversation id of the call</param>
        ///// <param name="recordingId">Recording id of the call</param>
        //[HttpGet]
        ////[Route("pauseRecording")]
        //public async Task<IHttpActionResult> PauseRecordingAsync(string serverCallId, string recordingId)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(serverCallId))
        //        {
        //            if (string.IsNullOrEmpty(recordingId))
        //            {
        //                recordingId = recordingData[serverCallId];
        //            }
        //            else
        //            {
        //                if (!recordingData.ContainsKey(serverCallId))
        //                {
        //                    recordingData[serverCallId] = recordingId;
        //                }
        //            }
        //            var pauseRecording = await callingServerClient.InitializeServerCall(serverCallId).PauseRecordingAsync(recordingId);
        //            //Logger.LogInformation($"PauseRecordingAsync response -- > {pauseRecording}");

        //            return Ok();
        //        }
        //        else
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Message = "serverCallId is invalid" });
        //            //return BadRequest(new { Message = "serverCallId is invalid" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Exception = ex });
        //    }
        //}

        ///// <summary>
        ///// Method to resume call recording
        ///// </summary>
        ///// <param name="serverCallId">Conversation id of the call</param>
        ///// <param name="recordingId">Recording id of the call</param>
        //[HttpGet]
        ////[Route("resumeRecording")]
        //public async Task<IHttpActionResult> ResumeRecordingAsync(string serverCallId, string recordingId)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(serverCallId))
        //        {
        //            if (string.IsNullOrEmpty(recordingId))
        //            {
        //                recordingId = recordingData[serverCallId];
        //            }
        //            else
        //            {
        //                if (!recordingData.ContainsKey(serverCallId))
        //                {
        //                    recordingData[serverCallId] = recordingId;
        //                }
        //            }
        //            var resumeRecording = await callingServerClient.InitializeServerCall(serverCallId).ResumeRecordingAsync(recordingId);
        //            //Logger.LogInformation($"ResumeRecordingAsync response -- > {resumeRecording}");

        //            return Ok();
        //        }
        //        else
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Message = "serverCallId is invalid" });
        //            //return BadRequest(new { Message = "serverCallId is invalid" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Exception = ex });
        //    }
        //}

        ///// <summary>
        ///// Method to stop call recording
        ///// </summary>
        ///// <param name="serverCallId">Conversation id of the call</param>
        ///// <param name="recordingId">Recording id of the call</param>
        ///// <returns></returns>
        //[HttpGet]
        ////[Route("stopRecording")]
        //public async Task<IHttpActionResult> StopRecordingAsync(string serverCallId, string recordingId)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(serverCallId))
        //        {
        //            if (string.IsNullOrEmpty(recordingId))
        //            {
        //                recordingId = recordingData[serverCallId];
        //            }
        //            else
        //            {
        //                if (!recordingData.ContainsKey(serverCallId))
        //                {
        //                    recordingData[serverCallId] = recordingId;
        //                }
        //            }

        //            var stopRecording = await callingServerClient.InitializeServerCall(serverCallId).StopRecordingAsync(recordingId).ConfigureAwait(false);
        //            //Logger.LogInformation($"StopRecordingAsync response -- > {stopRecording}");

        //            if (recordingData.ContainsKey(serverCallId))
        //            {
        //                recordingData.Remove(serverCallId);
        //            }
        //            return Ok();
        //        }
        //        else
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Message = "serverCallId is invalid" });
        //            //return BadRequest(new { Message = "serverCallId is invalid" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Exception = ex });
        //    }
        //}

        ///// <summary>
        ///// Method to get recording state
        ///// </summary>
        ///// <param name="serverCallId">Conversation id of the call</param>
        ///// <param name="recordingId">Recording id of the call</param>
        ///// <returns>
        ///// CallRecordingProperties
        /////     RecordingState is {active}, in case of active recording
        /////     RecordingState is {inactive}, in case if recording is paused
        ///// 404:Recording not found, if recording was stopped or recording id is invalid.
        ///// </returns>
        //[HttpGet]
        ////[Route("getRecordingState")]
        //public async Task<IHttpActionResult> GetRecordingState(string serverCallId, string recordingId)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(serverCallId))
        //        {
        //            if (string.IsNullOrEmpty(recordingId))
        //            {
        //                recordingId = recordingData[serverCallId];
        //            }
        //            else
        //            {
        //                if (!recordingData.ContainsKey(serverCallId))
        //                {
        //                    recordingData[serverCallId] = recordingId;
        //                }
        //            }

        //            var recordingState = await callingServerClient.InitializeServerCall(serverCallId).GetRecordingStateAsync(recordingId).ConfigureAwait(false);

        //            //Logger.LogInformation($"GetRecordingStateAsync response -- > {recordingState}");

        //            return Json(recordingState.Value.RecordingState);
        //        }
        //        else
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Message = "serverCallId is invalid" });
        //            //return BadRequest(new { Message = "serverCallId is invalid" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Exception = ex });
        //    }
        //}
    }
}

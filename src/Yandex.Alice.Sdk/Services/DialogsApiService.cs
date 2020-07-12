﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yandex.Alice.Sdk.Models.DialogsApi;
using Yandex.Alice.Sdk.Services.Interfaces;

namespace Yandex.Alice.Sdk.Services
{
    public class DialogsApiService : IDialogsApiService, IDisposable
    {
        private readonly HttpClient _dialogsApiClient;

        public DialogsApiService(DialogsApiSettings dialogsApiSettings)
        {
            if(dialogsApiSettings == null)
            {
                throw new ArgumentNullException(nameof(dialogsApiSettings));
            }

            _dialogsApiClient = new HttpClient()
            {
                BaseAddress = new Uri("https://dialogs.yandex.net")
            };
            _dialogsApiClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("OAuth", dialogsApiSettings.DialogsOAuthToken);
        }

        #region Image       

        public async Task<DialogsApiResponse<DialogsImageUploadResponse>> UploadImageAsync(Guid skillId, DialogsWebUploadRequest request)
        {
            string requestUri = $"/api/v1/skills/{skillId}/images";
            string json = JsonSerializer.Serialize(request);
            using (HttpContent requestContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = requestContent
                    })
                {
                    return await SendAsync<DialogsImageUploadResponse>(requestMessage).ConfigureAwait(false);
                }
            }
        }        

        public async Task<DialogsApiResponse<DialogsImageUploadResponse>> UploadImageAsync(Guid skillId, DialogsFileUploadRequest request)
        {
            string url = $"/api/v1/skills/{skillId}/images";
            return await SendFileAsync<DialogsImageUploadResponse>(url, request).ConfigureAwait(false);
        }


        public async Task<DialogsApiResponse<DialogsImagesInfoList>> GetImagesAsync(Guid skillId)
        {
            string url = $"/api/v1/skills/{skillId}/images";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return await SendAsync<DialogsImagesInfoList>(requestMessage).ConfigureAwait(false);
            }            
        }

        public async Task<DialogsApiResponse<DialogsDeleteResponse>> DeleteImageAsync(Guid skillId, string imageId)
        {
            string url = $"/api/v1/skills/{skillId}/images/{imageId}";
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                return await SendAsync<DialogsDeleteResponse>(httpRequestMessage).ConfigureAwait(false);
            }            
        }

        #endregion

        #region Sound

        public async Task<DialogsApiResponse<DialogsSoundResponse>> UploadSoundAsync(Guid skillId, DialogsFileUploadRequest request)
        {
            string url = $"/api/v1/skills/{skillId}/sounds";
            return await SendFileAsync<DialogsSoundResponse>(url, request).ConfigureAwait(false);
        }


        public async Task<DialogsApiResponse<DialogsSoundResponse>> GetSoundAsync(Guid skillId, Guid soundId)
        {
            string url = $"/api/v1/skills/{skillId}/sounds/{soundId}";
            using(var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return await SendAsync<DialogsSoundResponse>(requestMessage).ConfigureAwait(false);
            }
        }

        public async Task<DialogsApiResponse<DialogsSoundsInfoList>> GetSoundsAsync(Guid skillId)
        {
            string url = $"/api/v1/skills/{skillId}/sounds";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return await SendAsync<DialogsSoundsInfoList>(requestMessage).ConfigureAwait(false);
            }
        }

        public async Task<DialogsApiResponse<DialogsDeleteResponse>> DeleteSoundAsync(Guid skillId, Guid soundId)
        {
            string url = $"/api/v1/skills/{skillId}/sounds/{soundId}";
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                return await SendAsync<DialogsDeleteResponse>(httpRequestMessage).ConfigureAwait(false);
            }
        }

        #endregion

        public async Task<DialogsApiResponse<DialogsStatus>> StatusAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/status"))
            {
                return await SendAsync<DialogsStatus>(requestMessage).ConfigureAwait(false);
            }
        }

        private async Task<DialogsApiResponse<TContent>> SendFileAsync<TContent>(string url, DialogsFileUploadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var streamContent = new StreamContent(request.Content))
            {
                using (var formContent = new MultipartFormDataContent
                    {
                        {streamContent,"file", request.FileName}
                    })
                {
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = formContent
                    }
                    )
                    {
                        return await SendAsync<TContent>(requestMessage).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<DialogsApiResponse<TContent>> SendAsync<TContent>(HttpRequestMessage message)
        {
            var apiResponse = await _dialogsApiClient.SendAsync(message).ConfigureAwait(false);
            string contentString = await apiResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            DialogsApiResponse<TContent> response;
            if (apiResponse.IsSuccessStatusCode)
            {
                var content = JsonSerializer.Deserialize<TContent>(contentString);
                response = new DialogsApiResponse<TContent>(content);
            }
            else if (apiResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                var content = JsonSerializer.Deserialize<DialogsResponseContent>(contentString);
                response = new DialogsApiResponse<TContent>(content.Message);
            }
            else
            {
                response = new DialogsApiResponse<TContent>(contentString);
            }
            return response;
        }

        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dialogsApiClient.Dispose();
                }

                disposedValue = true;
            }
        }

        // ~DialogsApiService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

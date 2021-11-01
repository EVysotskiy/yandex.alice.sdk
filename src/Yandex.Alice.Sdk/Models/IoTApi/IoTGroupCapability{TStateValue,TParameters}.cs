﻿namespace Yandex.Alice.Sdk.Models.IoTApi
{
    using System.Text.Json.Serialization;
    using Yandex.Alice.Sdk.Models.SmartHome;

    public abstract class IoTGroupCapability<TStateValue, TParameters> : IoTGroupCapability
        where TStateValue : SmartHomeDeviceCapabilityStateValue
        where TParameters : SmartHomeDeviceCapabilityParameters
    {
        [JsonPropertyName("state")]
        public TStateValue State { get; set; }

        [JsonPropertyName("parameters")]
        public TParameters Parameters { get; set; }
    }
}
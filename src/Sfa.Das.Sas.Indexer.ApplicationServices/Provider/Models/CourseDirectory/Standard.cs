﻿// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator 0.9.7.0
// Changes may cause incorrect behavior and will be lost if the code is regenerated.

using System.Collections.Generic;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;

namespace Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Models.CourseDirectory
{
    public class Standard
    {
        /// <summary>
        ///     Initializes a new instance of the Standard class.
        /// </summary>
        public Standard()
        {
            Locations = new LazyList<LocationRef>();
        }

        /// <summary>
        ///     Initializes a new instance of the Standard class with required
        ///     arguments.
        /// </summary>
        public Standard(int standardCode)
            : this()
        {
            StandardCode = standardCode;
        }

        /// <summary>
        ///     Optional.
        /// </summary>
        public Contact Contact { get; set; }

        /// <summary>
        ///     Optional.
        /// </summary>
        public IList<LocationRef> Locations { get; set; }

        /// <summary>
        ///     Optional.
        /// </summary>
        public string MarketingInfo { get; set; }

        /// <summary>
        ///     Required.
        /// </summary>
        public int StandardCode { get; set; }

        /// <summary>
        ///     Optional.
        /// </summary>
        public string StandardInfoUrl { get; set; }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        public virtual void DeserializeJson(JToken inputObject)
        {
            if (inputObject != null && inputObject.Type != JTokenType.Null)
            {
                var contactValue = inputObject["contact"];
                if (contactValue != null && contactValue.Type != JTokenType.Null)
                {
                    var contact = new Contact();
                    contact.DeserializeJson(contactValue);
                    Contact = contact;
                }

                var locationsSequence = inputObject["locations"];
                if (locationsSequence != null && locationsSequence.Type != JTokenType.Null)
                {
                    foreach (var locationsValue in (JArray)locationsSequence)
                    {
                        var locationRef = new LocationRef();
                        locationRef.DeserializeJson(locationsValue);
                        Locations.Add(locationRef);
                    }
                }

                var marketingInfoValue = inputObject["marketingInfo"];
                if (marketingInfoValue != null && marketingInfoValue.Type != JTokenType.Null)
                {
                    MarketingInfo = (string)marketingInfoValue;
                }
                var standardCodeValue = inputObject["standardCode"];
                if (standardCodeValue != null && standardCodeValue.Type != JTokenType.Null)
                {
                    StandardCode = (int)standardCodeValue;
                }
                var standardInfoUrlValue = inputObject["standardInfoUrl"];
                if (standardInfoUrlValue != null && standardInfoUrlValue.Type != JTokenType.Null)
                {
                    StandardInfoUrl = (string)standardInfoUrlValue;
                }
            }
        }
    }
}
﻿#region License

/*
 * Copyright 2002-2012 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Net;
using Spring.Http;
using Spring.Rest.Client;
using Spring.Rest.Client.Support;

namespace CSharp.Meetup.Api.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IResponseErrorHandler"/> that handles errors from Meetup's REST API, 
    /// interpreting them into appropriate exceptions.
    /// </summary>
	/// <author>Scott Smith</author>
    class MeetupErrorHandler : DefaultResponseErrorHandler
    {
    	/// <summary>
        /// Handles the error in the given response. 
        /// <para/>
        /// This method is only called when HasError() method has returned <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// This implementation throws appropriate exception if the response status code 
        /// is a client code error (4xx) or a server code error (5xx). 
        /// </remarks>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="requestMethod">The request method.</param>
        /// <param name="response">The response message with the error.</param>
        public override void HandleError(Uri requestUri, HttpMethod requestMethod, HttpResponseMessage<byte[]> response)
        {
			if (response == null) throw new ArgumentNullException("response");

            var type = (int)response.StatusCode / 100;
            switch (type)
            {
            	case 4:
            		HandleClientErrors(response.StatusCode);
            		break;
            	case 5:
            		HandleServerErrors(response.StatusCode);
            		break;
            }

            // if not otherwise handled, do default handling and wrap with MeetupApiException
            try
            {
                base.HandleError(requestUri, requestMethod, response);
            }
            catch (Exception ex)
            {
                throw new MeetupApiException("Error consuming Meetup REST API.", ex);
            }
        }

		private void HandleClientErrors(HttpStatusCode statusCode)
        {
			throw new MeetupApiException(
				"The server indicated a client error has occured and returned the following HTTP status code: " + statusCode,
				MeetupApiError.ClientError);
        }

    	private void HandleServerErrors(HttpStatusCode statusCode)
        {
            throw new MeetupApiException(
				"The server indicated a server error has occured and returned the following HTTP status code: " + statusCode,
                MeetupApiError.ServerError);
	    }
    }
}
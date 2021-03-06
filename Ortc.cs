using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ModernHttpClient;
using RealtimeFramework.Messaging.Ext;
using RealtimeFramework.Messaging.Exceptions;


namespace RealtimeFramework.Messaging {
    /// <summary>
    /// Class with static methods for authentication and presence management.
    /// </summary>
    public static class Ortc {
        /// <summary>
        /// The channel permission.
        /// </summary>
        public enum ChannelPermissions {
            /// <summary>
            /// Read permission
            /// </summary>
            Read = 'r',

            /// <summary>
            /// Read and Write permission
            /// </summary>
            Write = 'w',

            /// <summary>
            /// Presence permission
            /// </summary>
            Presence = 'p'
        }


        /// <summary>
        /// Saves the authentication token channels permissions in the ORTC server.
        /// </summary>
        /// <param name="url">ORTC server URL.</param>
        /// <param name="isCluster">Indicates whether the ORTC server is in a cluster.</param>
        /// <param name="authenticationToken">Authentication Token which is generated by the application server, for instance a unique session ID.</param>
        /// <param name="authenticationTokenIsPrivate">Indicates whether the authentication token is private (1) or not (0).</param>
        /// <param name="applicationKey">Application Key that was provided to you together with the ORTC service purchasing.</param>
        /// <param name="timeToLive">The authentication token time to live, in other words, the allowed activity time (in seconds).</param>
        /// <param name="privateKey">The private key provided to you together with the ORTC service purchasing.</param>
        /// <param name="permissions">The channels and their permissions (w: write/read or r: read, case sensitive).</param>
        /// <returns>True if the authentication was successful or false if it was not.</returns>
        /// <exception cref="OrtcEmptyFieldException">Server URL can not be null or empty.</exception>
        /// <exception cref="OrtcAuthenticationNotAuthorizedException">Unauthorized by the server.</exception>
        /// <exception cref="OrtcNotConnectedException">Unable to connect to the authentication server.</exception>
        /// <example>
        /// <code>
        ///    // Permissions
        ///    Dictionary;string, ChannelPermissions; permissions = new Dictionary;string, ChannelPermissions;();
        /// 
        ///    permissions.Add("channel1", ChannelPermissions.Read);
        ///    permissions.Add("channel2", ChannelPermissions.Write);
        /// 
        ///    string url = "http://ortc-developers.realtime.co/server/2.1"; 
        ///    bool isCluster = true;
        ///    string authenticationToken = "myAuthenticationToken";
        ///    bool authenticationTokenIsPrivate = true;
        ///    string applicationKey = "myApplicationKey";
        ///    int timeToLive = 1800; // 30 minutes
        ///    string privateKey = "myPrivateKey";
        /// 
        ///    bool authSaved = Ibt.Ortc.Api.Ortc.SaveAuthentication(url, isCluster, authenticationToken, authenticationTokenIsPrivate, applicationKey, timeToLive, privateKey, permissions)) 
        /// </code>
        /// </example>
        public static bool SaveAuthentication(string url, bool isCluster, string authenticationToken, bool authenticationTokenIsPrivate,
                                              string applicationKey, int timeToLive, string privateKey, Dictionary<string, ChannelPermissions> permissions) {
            var result = false;

            if (permissions != null && permissions.Count > 0) {
                var multiPermissions = new Dictionary<string, List<ChannelPermissions>>();

                foreach (var permission in permissions) {
                    var permissionList = new List<ChannelPermissions>();
                    permissionList.Add(permission.Value);

                    multiPermissions.Add(permission.Key, permissionList);
                }

                result = Ortc.SaveAuthentication(url, isCluster, authenticationToken, authenticationTokenIsPrivate, applicationKey, timeToLive, privateKey, multiPermissions);
            }

            return result;
        }

        /// <summary>
        /// Saves the authentication token channels permissions in the ORTC server.
        /// </summary>
        /// <param name="url">ORTC server URL.</param>
        /// <param name="isCluster">Indicates whether the ORTC server is in a cluster.</param>
        /// <param name="authenticationToken">Authentication Token which is generated by the application server, for instance a unique session ID.</param>
        /// <param name="authenticationTokenIsPrivate">Indicates whether the authentication token is private (1) or not (0).</param>
        /// <param name="applicationKey">Application Key that was provided to you together with the ORTC service purchasing.</param>
        /// <param name="timeToLive">The authentication token time to live, in other words, the allowed activity time (in seconds).</param>
        /// <param name="privateKey">The private key provided to you together with the ORTC service purchasing.</param>
        /// <param name="permissions">The channels and their permissions (w: write/read or r: read, case sensitive).</param>
        /// <returns>True if the authentication was successful or false if it was not.</returns>
        /// <exception cref="OrtcEmptyFieldException">Server URL can not be null or empty.</exception>
        /// <exception cref="OrtcAuthenticationNotAuthorizedException">Unauthorized by the server.</exception>
        /// <exception cref="OrtcNotConnectedException">Unable to connect to the authentication server.</exception>
        /// <example>
        /// <code>
        ///    // Permissions
        ///    Dictionary;string, ChannelPermissions; permissions = new Dictionary;string, List;ChannelPermissions;;();
        /// 
        ///    Dictionary;string, List;ChannelPermissions;; channelPermissions = new Dictionary;string, List;ChannelPermissions;;();
        ///    var permissionsList = new List;ChannelPermissions;();
        /// 
        ///    permissionsList.Add(ChannelPermissions.Write);
        ///    permissionsList.Add(ChannelPermissions.Presence);
        ///    
        ///    channelPermissions.Add("channel1", permissionsList);
        /// 
        ///    string url = "http://ortc-developers.realtime.co/server/2.1"; 
        ///    bool isCluster = true;
        ///    string authenticationToken = "myAuthenticationToken";
        ///    bool authenticationTokenIsPrivate = true;
        ///    string applicationKey = "myApplicationKey";
        ///    int timeToLive = 1800; // 30 minutes
        ///    string privateKey = "myPrivateKey";
        /// 
        ///    bool authSaved = Ibt.Ortc.Api.Ortc.SaveAuthentication(url, isCluster, authenticationToken, authenticationTokenIsPrivate, applicationKey, timeToLive, privateKey, channelPermissions)) 
        /// </code>
        /// </example>
        public static bool SaveAuthentication(string url, bool isCluster, string authenticationToken, bool authenticationTokenIsPrivate,
                                              string applicationKey, int timeToLive, string privateKey, Dictionary<string, List<ChannelPermissions>> permissions) {

            if (String.IsNullOrEmpty(url)) {
                throw new OrtcEmptyFieldException("URL is null or empty.");
            } else if (String.IsNullOrEmpty(applicationKey)) {
                throw new OrtcEmptyFieldException("Application Key is null or empty.");
            } else if (String.IsNullOrEmpty(authenticationToken)) {
                throw new OrtcEmptyFieldException("Authentication Token is null or empty.");
            } else if (String.IsNullOrEmpty(privateKey)) {
                throw new OrtcEmptyFieldException("Private Key is null or empty.");
            } 


            string connectionUrl = url;

            if (isCluster) {
                connectionUrl = Balancer.ResolveClusterUrl(url);    
            }

            if (String.IsNullOrEmpty(connectionUrl)) {
                throw new OrtcNotConnectedException("Unable to get URL from cluster");
            }

            connectionUrl = connectionUrl.EndsWith("/") ? connectionUrl + "authenticate" : connectionUrl + "/authenticate";

            string postParameters = String.Format("AT={0}&PVT={1}&AK={2}&TTL={3}&PK={4}", authenticationToken, authenticationTokenIsPrivate ? 1 : 0, applicationKey, timeToLive, privateKey);

            if (permissions != null && permissions.Count > 0) {
                postParameters += String.Format("&TP={0}", permissions.Count);
                foreach (var permission in permissions) {
                    var permissionItemText = String.Format("{0}=", permission.Key);
                    foreach (var permissionItemValue in permission.Value.ToList()) {
                        permissionItemText += ((char)permissionItemValue).ToString();
                    }

                    postParameters += String.Format("&{0}", permissionItemText);
                }
            }



            HttpContent content = new StringContent(postParameters);
            using (var client = new HttpClient(new NativeMessageHandler())) {
                var response = client.PostAsync(new Uri(connectionUrl), content).Result;

                if (response.IsSuccessStatusCode) {
                    return true;
                } else {
                    throw new OrtcAuthenticationNotAuthorizedException(response.Content.ReadAsStringAsync().Result);
                }
            }
        }



        /// <summary>
        /// Gets the subscriptions in the specified channel and if active the first 100 unique metadata.
        /// </summary>
        /// <param name="url">Server containing the presence service.</param>
        /// <param name="isCluster">Specifies if url is cluster</param>
        /// <param name="applicationKey">Application key with access to presence service.</param>
        /// <param name="authenticationToken">Authentication token with access to presence service.</param>
        /// <param name="channel">Channel with presence data active.</param>
        /// <param name="callback"><see cref="OnPresenceDelegate"/>Callback with error <see cref="OrtcPresenceException"/> and result <see cref="Presence"/>.</param>
        /// <example>
        /// <code>
        /// client.Presence("presence-channel", (error, result) =>
        /// {
        ///     if (error != null)
        ///     {
        ///         System.Diagnostics.Debug.Writeline(error.Message);
        ///     }
        ///     else
        ///     {
        ///         if (result != null)
        ///         {
        ///             System.Diagnostics.Debug.Writeline(result.Subscriptions);
        /// 
        ///             if (result.Metadata != null)
        ///             {
        ///                 foreach (var metadata in result.Metadata)
        ///                 {
        ///                     System.Diagnostics.Debug.Writeline(metadata.Key + " - " + metadata.Value);
        ///                 }
        ///             }
        ///         }
        ///         else
        ///         {
        ///             System.Diagnostics.Debug.Writeline("There is no presence data");
        ///         }
        ///     }
        /// });
        /// </code>
        /// </example>
        public static void Presence(String url, Boolean isCluster, String applicationKey, String authenticationToken, String channel, OnPresenceDelegate callback) {
            Ext.Presence.GetPresence(url, isCluster, applicationKey, authenticationToken, channel, callback);
        }

        /// <summary>
        /// Enables presence for the specified channel with first 100 unique metadata if metadata is set to true.
        /// </summary>
        /// <param name="url">Server containing the presence service.</param>
        /// <param name="isCluster">Specifies if url is cluster</param>
        /// <param name="applicationKey">Application key with access to presence service.</param>
        /// <param name="privateKey">The private key provided when the ORTC service is purchased.</param>
        /// <param name="channel">Channel to activate presence.</param>
        /// <param name="metadata">Defines if to collect first 100 unique metadata.</param>
        /// <param name="callback">Callback with error <see cref="OrtcPresenceException"/> and result.</param>
        /// /// <example>
        /// <code>
        /// client.EnablePresence("myPrivateKey", "presence-channel", false, (error, result) =>
        /// {
        ///     if (error != null)
        ///     {
        ///         System.Diagnostics.Debug.Writeline(error.Message);
        ///     }
        ///     else
        ///     {
        ///         System.Diagnostics.Debug.Writeline(result);
        ///     }
        /// });
        /// </code>
        /// </example>
        public static void EnablePresence(String url, Boolean isCluster, String applicationKey, String privateKey, String channel, bool metadata, OnEnablePresenceDelegate callback) {
            Ext.Presence.EnablePresence(url, isCluster, applicationKey, privateKey, channel, metadata, callback);
        }

        /// <summary>
        /// Disables presence for the specified channel.
        /// </summary>
        /// <param name="url">Server containing the presence service.</param>
        /// <param name="isCluster">Specifies if url is cluster</param>
        /// <param name="applicationKey">Application key with access to presence service.</param>
        /// <param name="privateKey">The private key provided when the ORTC service is purchased.</param>
        /// <param name="channel">Channel to disable presence.</param>
        /// <param name="callback">Callback with error <see cref="OrtcPresenceException"/> and result.</param>        
        public static void DisablePresence(String url, Boolean isCluster, String applicationKey, String privateKey, String channel, OnDisablePresenceDelegate callback) {
            Ext.Presence.DisablePresence(url, isCluster, applicationKey, privateKey, channel, callback);
        }
    }
}

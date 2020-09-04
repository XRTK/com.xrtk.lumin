//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Debug = UnityEngine.Debug;

namespace XRTK.Lumin.Native
{
    using System.Runtime.InteropServices;

    internal static class MlApi
    {
        /// <summary>
        /// Maximum character length of MLVersion build ID
        /// </summary>
        public const int MLVersion_BuildIdMaxLength = 128;

        /// <summary>
        /// SDK version: '[major][minor][revision][build_id]'
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLVersion
        {
            /// <summary>
            /// Major version
            /// </summary>
            public uint major;

            /// <summary>
            /// Minor version
            /// </summary>
            public uint minor;

            /// <summary>
            /// Revision
            /// </summary>
            public uint revision;

            /// <summary>
            /// Build ID
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string build_id;
        }

        /// <summary>
        /// Integer handle type used to reference many things returned by the API, instead of
        /// returning pointers directly Use MLHandleIsValid to check if a handle is valid or not
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct MLHandle : IEquatable<MLHandle>
        {
            public MLHandle(ulong value) => this.Value = value;

            public readonly ulong Value;

            public bool IsValid => Value != ulong.MaxValue && Value > 0L;

            public bool Equals(MLHandle other) => Value.Equals(other.Value);

            public override bool Equals(object obj) => obj is MLHandle other && Equals(other);

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString();

            public static implicit operator ulong(MLHandle from) => from.Value;

            public static implicit operator MLHandle(ulong from) => new MLHandle(from);

            public static bool operator ==(MLHandle left, MLHandle right) => left.Equals(right);

            public static bool operator !=(MLHandle left, MLHandle right) => !left.Equals(right);
        }

        /// <summary>
        /// MLResult type that all functions in the APIs return
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct MLResult : IEquatable<MLResult>
        {
            /// <summary>
            /// Identifier of Magic Leap API results
            /// </summary>
            public enum Code : int
            {
                /// <summary>
                /// Operation completed successfully.
                /// </summary>
                Ok = (CodePrefix.MLResultGlobal << 16),

                /// <summary>
                /// Asynchronous operation has not completed
                /// </summary>
                Pending,

                /// <summary>
                /// Operation has timed out.
                /// </summary>
                Timeout,

                /// <summary>
                /// Request to lock a shared resource that is already locked.
                /// </summary>
                Locked,

                /// <summary>
                /// Operation failed due to an unspecified internal error.
                /// </summary>
                UnspecifiedFailure,

                /// <summary>
                /// Operation failed due to an invalid parameter being supplied.
                /// </summary>
                InvalidParam,

                /// <summary>
                /// Operation failed because memory failed to be allocated.
                /// </summary>
                AllocFailed,

                /// <summary>
                /// Operation failed because a required privilege has not been granted.
                /// </summary>
                PrivilegeDenied,

                /// <summary>
                /// Operation failed because it is not currently implemented.
                /// </summary>
                NotImplemented,

                // MLAudioResult

                /// <summary>
                /// Audio function not implemented.
                /// </summary>
                AudioNotImplemented = (CodePrefix.MLAudioResult << 16),

                /// <summary>
                /// Not a valid MLHandle for a sound or input.
                /// </summary>
                AudioHandleNotFound,

                /// <summary>
                /// Sample rate not supported.
                /// </summary>
                AudioInvalidSampleRate,

                /// <summary>
                /// Bits per sample not supported.
                /// </summary>
                AudioInvalidBitsPerSample,

                /// <summary>
                /// Valid bits per sample not supported.
                /// </summary>
                AudioInvalidValidBits,

                /// <summary>
                /// Sample format not supported.
                /// </summary>
                AudioInvalidSampleFormat,

                /// <summary>
                /// Channel count not supported.
                /// </summary>
                AudioInvalidChannelCount,

                /// <summary>
                /// Buffer size too small.
                /// </summary>
                AudioInvalidBufferSize,

                /// <summary>
                /// Buffer not ready for read or write.
                /// </summary>
                AudioBufferNotReady,

                /// <summary>
                /// Specified file not found.
                /// </summary>
                AudioFileNotFound,

                /// <summary>
                /// Specified file has unsupported format.
                /// </summary>
                AudioFileNotRecognized,

                /// <summary>
                /// Specified resource is not on the list.
                /// </summary>
                AudioResourceNotFound,

                /// <summary>
                /// Data was unloaded or file was closed.
                /// </summary>
                AudioResourceDiscarded,

                /// <summary>
                /// Requested operation not possible for given item.
                /// </summary>
                AudioOperationUnavailable,

                /// <summary>
                /// Internal configuration problem was detected.
                /// </summary>
                AudioInternalConfigError,

                // MLMediaPlayerResult

                /// <summary>
                /// Media errors (example: Codec not supported).
                /// </summary>
                MediaPlayerServerDied = (CodePrefix.MLMediaPlayerResult << 16),

                /// <summary>
                /// Runtime errors.
                /// </summary>
                MediaPlayerNotValidForProgressivePlayback,

                /// <summary>
                /// Media not connected.
                /// </summary>
                MediaNotConnected = (CodePrefix.MLMediaResult << 16),

                /// <summary>
                /// Media had unknown host.
                /// </summary>
                MediaUnknownHost,

                /// <summary>
                /// Media cannot connect.
                /// </summary>
                MediaCannotConnect,

                /// <summary>
                /// Media IO.
                /// </summary>
                MediaIO,

                /// <summary>
                /// Media connection was lost.
                /// </summary>
                MediaConnectionLost,

                /// <summary>
                /// Deprecated error.
                /// </summary>
                MediaLegacy1,

                /// <summary>
                /// Media was malformed.
                /// </summary>
                MediaMalformed,

                /// <summary>
                /// Media was out of range.
                /// </summary>
                MediaOutOfRange,

                /// <summary>
                /// Media buffer was too small.
                /// </summary>
                MediaBufferTooSmall,

                /// <summary>
                /// Media not supported.
                /// </summary>
                MediaUnsupported,

                /// <summary>
                /// Media end of stream.
                /// </summary>
                MediaEndOfStream,

                /// <summary>
                /// Media format changed.
                /// </summary>
                MediaFormatChanged,

                /// <summary>
                /// Media discontinuity.
                /// </summary>
                MediaDiscontinuity,

                /// <summary>
                /// Media output buffers changed.
                /// </summary>
                MediaOutputBuffersChanged,

                /// <summary>
                /// Media permission revoked.
                /// </summary>
                MediaPermissionRevoked,

                /// <summary>
                /// Media had an unsupported audio format.
                /// </summary>
                MediaUnsupportedAudioFormat,

                /// <summary>
                /// Media heartbeat requested to be terminated.
                /// </summary>
                MediaHeartbeatTerminateRequested,

                // MLMediaDRMResult

                /// <summary>
                /// Error code for undefined type.
                /// </summary>
                MediaDRMUnknown = (CodePrefix.MLMediaDRMResult << 16),

                /// <summary>
                /// Error code for no DRM license.
                /// </summary>
                MediaDRMNoLicense,

                /// <summary>
                /// Error code for DRM license expired.
                /// </summary>
                MediaDRMLicenseExpired,

                /// <summary>
                /// Error code for DRM session not expired.
                /// </summary>
                MediaDRMSessionNotOpened,

                /// <summary>
                /// Error code for DRM when decrypt unit is not initialized.
                /// </summary>
                MediaDRMDecryptUnitNotInitialized,

                /// <summary>
                /// Error code for DRM when failed to decrypt data.
                /// </summary>
                MediaDRMDecrypt,

                /// <summary>
                /// Error code for DRM can not handle the operation.
                /// </summary>
                MediaDRMCannotHandle,

                /// <summary>
                /// Error code for DRM when data is tampered.
                /// </summary>
                MediaDRMTamperDetect,

                /// <summary>
                /// Error Code when an operation on a MLMediaDRM handle is attempted and the device does not have a certificate.
                /// The app should obtain and install a certificate using the MLMediaDRM provisioning methods then retry the operation.
                /// </summary>
                MediaDRMNotProvisioned,

                /// <summary>
                /// Error code for Device License Revoked.
                /// </summary>
                MediaDRMDeviceRevoked,

                /// <summary>
                /// Error code if the MLMediaDRM operation fails when the required resources are in use.
                /// </summary>
                MediaDRMResourceBusy,

                /// <summary>
                /// Error code for insufficient output protection.
                /// </summary>
                MediaDRMInsufficientOutputProtection,

                /// <summary>
                /// Error code for insufficient output protection.
                /// </summary>
                MediaDRMLastUsedErrorCode = MediaDRMInsufficientOutputProtection,

                /// <summary>
                /// Range for vendor specific DRM errors.
                /// </summary>
                MediaDRMVendorMin = (CodePrefix.MLMediaDRMResult << 16) + 500,

                /// <summary>
                /// Range for vendor specific DRM errors.
                /// </summary>
                MediaDRMVendorMax = (CodePrefix.MLMediaDRMResult << 16) + 999,

                // MLMediaGenericResult

                /// <summary>
                /// Media invalid operation.
                /// </summary>
                MediaGenericInvalidOperation = (CodePrefix.MLMediaGenericResult << 16),

                /// <summary>
                /// Media bad type.
                /// </summary>
                MediaGenericBadType,

                /// <summary>
                /// Media name not found.
                /// </summary>
                MediaGenericNameNotFound,

                /// <summary>
                /// Media handle not found.
                /// </summary>
                MediaGenericHandleNotFound,

                /// <summary>
                /// Media <c>NoInit</c>.
                /// </summary>
                MediaGenericNoInit,

                /// <summary>
                /// Media already exists.
                /// </summary>
                MediaGenericAlreadyExists,

                /// <summary>
                /// Media dead object.
                /// </summary>
                MediaGenericDeadObject,

                /// <summary>
                /// Media had a failed transaction.
                /// </summary>
                MediaGenericFailedTransaction,

                /// <summary>
                /// Media had a bad index.
                /// </summary>
                MediaGenericBadIndex,

                /// <summary>
                /// Media not enough data.
                /// </summary>
                MediaGenericNotEnoughData,

                /// <summary>
                /// Media would block.
                /// </summary>
                MediaGenericWouldBlock,

                /// <summary>
                /// Media had an unknown transaction.
                /// </summary>
                MediaGenericUnknownTransaction,

                /// <summary>
                /// Media FDS not allowed.
                /// </summary>
                MediaGenericFDSNotAllowed,

                /// <summary>
                /// Media unexpected null.
                /// </summary>
                MediaGenericUnexpectedNull,

                // MLDispatchResult

                /// <summary>
                /// Cannot start app.
                /// </summary>
                DispatchCannotStartApp = (CodePrefix.MLDispatchResult << 16),

                /// <summary>
                /// Invalid packet.
                /// </summary>
                DispatchInvalidPacket,

                /// <summary>
                /// No app found.
                /// </summary>
                DispatchNoAppFound,

                /// <summary>
                /// App packet dialog failure.
                /// </summary>
                DispatchAppPickerDialogFailure,

                /// <summary>
                /// Invalid Schema.
                /// </summary>
                DispatchInvalidSchema,

                /// <summary>
                /// Invalid Url.
                /// </summary>
                DispatchInvalidUrl,

                /// <summary>
                /// Schema Already Registered.
                /// </summary>
                DispatchSchemaAlreadyRegistered,

                // MLIdentityResult

                /// <summary>
                /// The local service is not running, or it cannot be accessed.
                /// </summary>
                IdentityFailedToConnectToLocalService = (CodePrefix.MLIdentityResult << 16),

                /// <summary>
                /// The service failed to access the cloud service.
                /// Either there is no IP connection or the cloud service is not available.
                /// </summary>
                IdentityFailedToConnectToCloudService,

                /// <summary>
                /// The user does not have the required privileges to use the requesting service
                /// or the refresh token used by the service is invalid.
                /// </summary>
                IdentityCloudAuthentication,

                /// <summary>
                /// Signature verification failed on the information returned by the cloud or a
                /// parsing error occurred.
                /// </summary>
                IdentityInvalidInformationFromCloud,

                /// <summary>
                /// The operation failed because the user is not logged in to the cloud.
                /// </summary>
                IdentityNotLoggedIn,

                /// <summary>
                /// The user's credentials have expired.
                /// </summary>
                IdentityExpiredCredentials,

                /// <summary>
                /// Failed to retrieve attributes of the user's profile.
                /// </summary>
                IdentityFailedToGetUserProfile,

                /// <summary>
                /// The cloud rejected the operation because the user is not authorized to execute it.
                /// </summary>
                IdentityUnauthorized,

                /// <summary>
                /// The device failed to authenticate the server.
                /// </summary>
                IdentityCertificateError,

                /// <summary>
                /// The cloud rejected the operation.
                /// </summary>
                IdentityRejectedByCloud,

                /// <summary>
                /// The user is already logged in.
                /// </summary>
                IdentityAlreadyLoggedIn,

                /// <summary>
                /// The cloud does not support modification of an attribute value.
                /// </summary>
                IdentityModifyIsNotSupported,

                /// <summary>
                /// The device is not connected to a network.
                /// </summary>
                IdentityNetworkError,

                // MLPassableWorldResult

                /// <summary>
                /// Low map quality.
                /// </summary>
                PassableWorldLowMapQuality = (CodePrefix.MLPassableWorldResult << 16),

                /// <summary>
                /// Unable to localize.
                /// </summary>
                PassableWorldUnableToLocalize,

                /// <summary>
                /// Server unavailable.
                /// </summary>
                PassableWorldServerUnavailable,

                /// <summary>
                /// Not found.
                /// </summary>
                PassableWorldNotFound,

                /// <summary>
                /// User has not enabled shared world in settings. Operation not available.
                /// </summary>
                PassableWorldSharedWorldNotEnabled,

                // MLTokenAgentResult

                /// <summary>
                /// The local binder service was not found.
                /// </summary>
                TokenAgentFailedToConnectToLocalService = (CodePrefix.MLTokenAgentResult << 16),

                /// <summary>
                /// The local binder service is running but has not been registered by the login service.
                /// </summary>
                TokenAgentServiceNotStarted,

                /// <summary>
                /// The local binder service failed to connect to the cloud service.
                /// </summary>
                TokenAgentFailedToConnectToCloud,

                /// <summary>
                /// The cloud service rejected the request due to inappropriate credentials.
                /// </summary>
                TokenAgentCloudAuthentication,

                /// <summary>
                /// The local server failed to log in with the cloud.
                /// </summary>
                TokenAgentFailedToLogin,

                /// <summary>
                /// An attempt to complete the login was begun without starting the login.
                /// </summary>
                TokenAgentLoginNotBegun,

                /// <summary>
                /// The operation was rejected because the local server was not logged in with the cloud.
                /// </summary>
                TokenAgentNotLoggedIn,

                /// <summary>
                /// The login request failed because the local server is already logged in with the cloud.
                /// </summary>
                TokenAgentAlreadyLoggedIn,

                /// <summary>
                /// The login request request failed because a login is already in progress.
                /// </summary>
                TokenAgentLoginInProgress,

                /// <summary>
                /// Having completed the login process with the cloud successfully, the service failed to
                /// start the local identity service.
                /// </summary>
                TokenAgentFailedToStartIdentityService,

                /// <summary>
                /// The serial number of the device is not recognized by the cloud so login failed.
                /// </summary>
                TokenAgentDeviceNotRegistered,

                /// <summary>
                /// The device is not yet connected to the cloud so login failed.
                /// </summary>
                TokenAgentDeviceNotConnected,

                // MLSnapshotResult

                /// <summary>
                /// Pose not found.
                /// </summary>
                SnapshotPoseNotFound = (CodePrefix.MLSnapshotResult << 16),

                // MLPrivilegesResult

                /// <summary>
                /// Used for MLPrivileges check and request privilege
                /// functions. Indicates if the user has decided to
                /// grant the privilege to the application.
                /// </summary>
                PrivilegeGranted = (CodePrefix.MLPrivilegesResult << 16),

                /// <summary>
                /// Used for MLPrivileges check and request privilege
                /// functions. Indicates whether a privilege has not
                /// yet been requested or if the user has decided not
                /// to grant the privilege to the application.
                /// </summary>
                PrivilegeNotGranted,

                // MLContactsResult

                /// <summary>
                /// This MLHandle is not yet recognized.
                /// </summary>
                ContactsHandleNotFound = (CodePrefix.MLContactsResult << 16),

                /// <summary>
                /// Request is completed, its corresponding result has been returned, and its related resources are pending to be freed.
                /// See MLContactsReleaseRequestResources().
                /// </summary>
                ContactsCompleted,

                /// <summary>
                /// Request is successfully cancelled.
                /// </summary>
                ContactsCancelled,

                /// <summary>
                /// Request failed due to system being in an illegal state, e.g., when the user hasn't successfully logged-in.
                /// </summary>
                ContactsIllegalState,

                // MLLocationResult

                /// <summary>
                /// Unknown location error.
                /// </summary>
                LocationUnknown = (CodePrefix.MLLocationResult << 16),

                /// <summary>
                /// No connection to server.
                /// </summary>
                LocationNetworkConnection,

                /// <summary>
                /// No location data received.
                /// </summary>
                LocationNoLocation,

                /// <summary>
                /// Location provider is not found.
                /// </summary>
                LocationProviderNotFound,

                // MLNetworkingResult

                /// <summary>
                /// The corresponding service is not available.
                /// </summary>
                NetworkingServiceNotAvailable = (CodePrefix.MLNetworkingResult << 16),

                /// <summary>
                /// The corresponding service returned with error.
                /// </summary>
                NetworkingServiceError,

                /// <summary>
                /// The version number in MLNetworkingWiFiData is not recognized.
                /// </summary>
                NetworkingWiFiDataStructureVersionError,

                /// <summary>
                /// WiFi service is not in the right state.
                /// </summary>
                NetworkingWiFiServiceInvalidState,

                // MLMovementResult

                /// <summary>
                /// Not a valid MLHandle for movement session.
                /// </summary>
                MovementInvalidMovementHandle = (CodePrefix.MLMovementResult << 16),

                /// <summary>
                /// Not a valid MLHandle for a collision session.
                /// </summary>
                MovementInvalidCollisionHandle,

                // MLConnectionsResult

                /// <summary>
                /// This MLHandle is not recognized.
                /// </summary>
                ConnectionsInvalidHandle = (CodePrefix.MLConnectionsResult << 16),

                /// <summary>
                /// Indicates number of invitees is invalid.
                /// </summary>
                ConnectionsInvalidInviteeCount,

                /// <summary>
                /// Indicates number of selectees is invalid.
                /// </summary>
                ConnectionsInvalidSelecteeCount = ConnectionsInvalidInviteeCount,

                /// <summary>
                /// Indicates invite request has been found and the system is attempting to cancel the process.
                /// </summary>
                ConnectionsCancellationPending,

                /// <summary>
                /// Request failed due to system being in an illegal state,
                /// e.g. user has not successfully logged-in.
                /// </summary>
                ConnectionsIllegalState,

                /// <summary>
                /// MLConnectionsRegisterForInvite failed because the system had an error
                /// with network connectivity, or the servers could not be reached.
                /// </summary>
                ConnectionsNetworkFailure,

                /// <summary>
                /// MLConnectionsRegisterForInvite failed because the application is already registered to handle invite requests.
                /// </summary>
                ConnectionsAlreadyRegistered,

                /// <summary>
                /// Indicates a general system failure
                /// </summary>
                ConnectionsSystemFailure,

                // MLSecureStorageResult

                /// <summary>
                /// Blob not found.
                /// </summary>
                SecureStorageBlobNotFound = (CodePrefix.MLSecureStorageResult << 16),

                /// <summary>
                /// I/O failure.
                /// </summary>
                SecureStorageIOFailure,

                /// <summary>
                /// Unsupported operation.
                /// </summary>
                AppConnectUnsupportedOperation = (CodePrefix.MLAppConnect << 16),

                /// <summary>
                /// Callback not removed.
                /// </summary>
                AppConnectCallbackNotRemoved,

                /// <summary>
                /// Application exists.
                /// </summary>
                AppConnectApplicationExists,

                /// <summary>
                /// Application does not exist.
                /// </summary>
                AppConnectApplicationDoesNotExist,

                /// <summary>
                /// Connection callback not registered.
                /// </summary>
                AppConnectConnectionCallbackNotRegistered,

                /// <summary>
                /// Connection exists.
                /// </summary>
                AppConnectConnectionExists,

                /// <summary>
                /// Connection does not exist.
                /// </summary>
                AppConnectConnectionDoesNotExist,

                /// <summary>
                /// Connections per app exceeded.
                /// </summary>
                AppConnectConnectionsPerAppExceeded,

                /// <summary>
                /// Connections per device exceeded.
                /// </summary>
                AppConnectConnectionsPerDeviceExceeded,

                /// <summary>
                /// Max data pipes for connection exceeded.
                /// </summary>
                AppConnectDataPipesPerConnectionExceeded,

                /// <summary>
                /// Pipe exists.
                /// </summary>
                AppConnectPipeExists,

                /// <summary>
                /// Pipe does not exist.
                /// </summary>
                AppConnectPipeDoesNotExist,

                /// <summary>
                /// Pipe type mismatch.
                /// </summary>
                AppConnectPipeTypeMismatch,

                /// <summary>
                /// Pipe closed.
                /// </summary>
                AppConnectPipeClosed,

                /// <summary>
                /// Max pipes per connection exceeded.
                /// </summary>
                AppConnectPipesPerConnectionExceeded,

                /// <summary>
                /// Pipe invalid operation.
                /// </summary>
                AppConnectPipeInvalidOperation,

                /// <summary>
                /// Pipe invalid properties.
                /// </summary>
                AppConnectPipeInvalidProperties,

                /// <summary>
                /// Pipe name reserved.
                /// </summary>
                AppConnectPipeNameReserved,

                /// <summary>
                /// Pipe buffer requested size exceeded limit.
                /// </summary>
                AppConnectPipeBufferSizeExceeded,

                /// <summary>
                /// Pipe buffer size can not be zero.
                /// </summary>
                AppConnectPipeBufferSizeZero,

                /// <summary>
                /// Pipe buffer can't be initialized.
                /// </summary>
                AppConnectPipeBufferInitError,

                /// <summary>
                /// Pipe buffer read overrun.
                /// </summary>
                AppConnectPipeBufferReadOverrun,

                /// <summary>
                /// Pipe buffer read error.
                /// </summary>
                AppConnectPipeBufferReadError,

                /// <summary>
                /// Pipe buffer write error.
                /// </summary>
                AppConnectPipeBufferWriteError,

                /// <summary>
                /// Pipe unknown priority.
                /// </summary>
                AppConnectPipeUnknownPriority,

                /// <summary>
                /// Invalid pipe large data header.
                /// </summary>
                AppConnectPipeLargeDataTimedOut,

                /// <summary>
                /// Pipe large data doesn't exist.
                /// </summary>
                AppConnectPipeLargeDataDoesNotExist,

                /// <summary>
                /// Microphone not enabled.
                /// </summary>
                AppConnectMicrophoneNotEnabled,

                /// <summary>
                /// Video frame not ready.
                /// </summary>
                AppConnectVideoFrameNotReady,

                /// <summary>
                /// Camera initialization fail.
                /// </summary>
                AppConnectCameraInitializationFail,

                /// <summary>
                /// [CAS] Cloud generic error.
                /// </summary>
                AppConnectCloudGenericError,

                /// <summary>
                /// [CAS] Contacts not invited.
                /// </summary>
                AppConnectCloudContactsNotInvited,

                /// <summary>
                /// [CAS] Credentials not valid.
                /// </summary>
                AppConnectCloudCredentialsNotValid,

                /// <summary>
                /// [CAS] <c>Copresence</c> session error.
                /// </summary>
                AppConnectCloudCopresenceSessionError,

                /// <summary>
                /// Contacts Provider request time out.
                /// </summary>
                AppConnectContactsProviderRequestError,

                /// <summary>
                /// Contact id already exists.
                /// </summary>
                AppConnectUserIdExists,

                /// <summary>
                /// Contact id does not exist.
                /// </summary>
                AppConnectUserIdInvalid,

                /// <summary>
                /// Friend Picker selection canceled.
                /// </summary>
                AppConnectInviteSelectionCancelledByUser,

                /// <summary>
                /// Friend Picker Invalid selection .
                /// </summary>
                AppConnecInviteSelectionInvalid,

                /// <summary>
                /// Friend Picker launch failed.
                /// </summary>
                AppConnectFriendPickerLaunchFail,

                /// <summary>
                /// Friend Picker launch/register time out.
                /// </summary>
                AppConnectFriendPickerLaunchRegisterTimeOut,

                /// <summary>
                /// Friend Picker Invalid argument.
                /// </summary>
                AppConnectFriendPickerInvalidArg
            }

            /// <summary>
            /// Result code prefixes for high order 2 byte prefix values used by the CAPI to group results by functionality. This is a <c>ushort</c> to facilitate bit shifting for final result values.
            /// </summary>
            private enum CodePrefix : ushort
            {
                /// <summary>
                /// Code for global MLResults.
                /// </summary>
                MLResultGlobal = 0x0000,

                /// <summary>
                /// Code for audio related MLResults.
                /// </summary>
                MLAudioResult = 0x9e11,

                /// <summary>
                /// Code for cloud related MLResults.
                /// </summary>
                MLCloudResult = 0xc4e3,

                /// <summary>
                /// Code for connections related MLResults..
                /// </summary>
                MLConnectionsResult = 0xbfae,

                /// <summary>
                /// Code for contacts related MLResults.
                /// </summary>
                MLContactsResult = 0x94a0,

                /// <summary>
                /// Code for dispatch related MLResults.
                /// </summary>
                MLDispatchResult = 0xBBE0,

                /// <summary>
                /// Code for identity related MLResults.
                /// </summary>
                MLIdentityResult = 0x7d4d,

                /// <summary>
                /// Code for location related MLResults.
                /// </summary>
                MLLocationResult = 0xda19,

                /// <summary>
                /// Code for mediaDRM related MLResults.
                /// </summary>
                MLMediaDRMResult = 0x62ce,

                /// <summary>
                /// Code for generic media related MLResults.
                /// </summary>
                MLMediaGenericResult = 0xbf3b,

                /// <summary>
                /// Code for media player related MLResults.
                /// </summary>
                MLMediaPlayerResult = 0xc435,

                /// <summary>
                /// Code for media related MLResults.
                /// </summary>
                MLMediaResult = 0x4184,

                /// <summary>
                /// Code for movement related MLResults.
                /// </summary>
                MLMovementResult = 0xdffe,

                /// <summary>
                /// Code for networking related MLResults.
                /// </summary>
                MLNetworkingResult = 0x4c62,

                /// <summary>
                /// Code for passable world related MLResults.
                /// </summary>
                MLPassableWorldResult = 0x41c7,

                /// <summary>
                /// Code for privileges related MLResults.
                /// </summary>
                MLPrivilegesResult = 0xcbcd,

                /// <summary>
                /// Code for purchase related MLResults.
                /// </summary>
                MLPurchaseResult = 0xdf1d,

                /// <summary>
                /// Code for secure storage related MLResults.
                /// </summary>
                MLSecureStorageResult = 0xba5c,

                /// <summary>
                /// Code for snapshot related MLResults.
                /// </summary>
                MLSnapshotResult = 0x87b8,

                /// <summary>
                /// Code for token agent related MLResults.
                /// </summary>
                MLTokenAgentResult = 0x37ee,

                /// <summary>
                /// Code for app connect related MLResults.
                /// </summary>
                MLAppConnect = 0xebf7
            }

            public MLResult(int value)
            {
                this.Value = (Code)value;
            }

            public readonly Code Value;

            public bool IsOk
            {
                get
                {
                    switch (Value)
                    {
                        case Code.Ok:
                            break;
                        case Code.Pending:
                            Debug.LogWarning("Result still pending, try again...");
                            break;
                        default:
                            Debug.LogError(Value);
                            break;
                    }

                    return Value == Code.Ok;
                }
            }

            public bool Equals(MLResult other) => Value.Equals(other.Value);

            public override bool Equals(object obj) => obj is MLResult other && Equals(other);

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString();

            public static implicit operator Code(MLResult from) => from.Value;

            public static implicit operator MLResult(int from) => new MLResult(from);

            public static bool operator ==(MLResult left, MLResult right) => left.Equals(right);

            public static bool operator !=(MLResult left, MLResult right) => !left.Equals(right);
        }
    }
}

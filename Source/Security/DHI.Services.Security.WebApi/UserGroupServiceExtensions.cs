namespace DHI.Services.Security.WebApi
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using Accounts;
    using Authorization;

    public static class UserGroupServiceExtensions
    {
        /// <summary>
        ///     Retrieves OTP configuration from user group metadata if available, otherwise the default OTP configuration, if provided.
        /// </summary>
        /// <param name="userGroupService">The user group service.</param>
        /// <param name="account">The user account.</param>
        /// <param name="twoFAMetadataKey">The metadata key used for 2FA authentication data.</param>
        /// <param name="defaultOtpConfig">Default OTP configuration applied if no metadata configuration is found.</param>
        public static string[] GetTwoFAMetadata(this UserGroupService userGroupService, Account account, string twoFAMetadataKey, string[] defaultOtpConfig = null)
        {
            var userGroupIds = userGroupService.GetIds(account.Id).ToArray();
            if (!userGroupIds.Any())
            {
                return defaultOtpConfig ?? Array.Empty<string>();
            }

            _ = userGroupService.TryGet(userGroupIds, out var userGroups);
            var userGroupsWithOtp = userGroups.Where(group => group is not null && group.Metadata.ContainsKey(twoFAMetadataKey))
                .ToArray();

            if (!userGroupsWithOtp.Any())
            {
                return defaultOtpConfig ?? Array.Empty<string>();
            }

            return userGroupsWithOtp
                .SelectMany(userGroup => userGroup.Metadata[twoFAMetadataKey].TransformJArray(userGroup.Name)).ToArray();
        }

        /// <summary>
        ///     Transforms OTP config in JArray form to type string[]
        /// </summary>
        /// <param name="original">An object containing OTP metadata.</param>
        /// <param name="userGroupName">The name of the user group.</param>
        /// <returns>Returns a clone of the original dictionary with OTP config transformed to string[].</returns>
        private static string[] TransformJArray(this object original, string userGroupName)
        {
            switch (original)
            {
                case string[] orig:
                    return orig;
                case JsonElement element when element.EnumerateArray().Any(x => x.ValueKind == JsonValueKind.String):
                    return element.EnumerateArray().Select(x => x.GetString()).ToArray();
                case JsonElement element when element.ValueKind == JsonValueKind.Array && element.GetArrayLength() == 0:
                    return Array.Empty<string>();
                default:
                    throw new ArgumentException($"2FA metadata on {userGroupName} with type {original.GetType().Name} is not valid. 2FA configuration must be in string[] form.");
            }
        }
    }
}
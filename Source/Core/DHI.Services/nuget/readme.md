## Core library for DHI Domain Services.

- v 13.0.0 - Replaced Newtonsoft.Json with System.Text.Json
- v 13.0.1 - Bug fix: Supported nullable property in query
- v 13.1.0-rc1 - Updated password policy
- v 13.1.0-rc2 - Added default converters for JsonLogger
- v 13.1.0-rc3 - Added DateRange custom converter
- v 14.0.0-rc1 - Lock account after multiple unsuccessful login attempts (support for IEC)
- v 14.0.0-rc2 - Added Password Expiry Policy (support for IEC)
- v 14.0.0-rc3 - Enable/Disable Accounts (support for IEC)
- v 14.0.0-rc4 - Bug fix in Validating Account Login Policy (support for IEC)
- v 15.0.0-rc1 - Replaced DHI ILogger with Microsoft ILogger
- v 15.0.0-rc4 - ILogger replacement using the Try pattern
- v 15.1.0-rc1 - IEC Authentication and ILogger Try Pattern Merged
- v 15.1.0-rc2 - IEC Authentication Bug Fixes
- v 15.1.0-rc3 - System.Text.Json Vurnerability Version Update
- v 15.1.0-rc4 - Security upgrade System.Text.Json from 8.0.4 to 8.0.5
- v 15.1.0 - System.Text.Json Vurnerability Version Update, Auth updates, Microsoft ILogger support
- v 16.0.0 - To add .NET 8.0 for the target
- v 16.1.0 - Adding ToList() when Deserialize to fix possible runtime exceptions when entities is being modified during iteration; Use private CTOR for JsonConstructor inside NotificationEntry
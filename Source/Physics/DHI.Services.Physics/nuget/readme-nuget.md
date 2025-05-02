# How to push a nuget package?
To push a new version of the nuget package, do the following:

1. Make the necessary changes to the `.nuspec` file (if any). Especially make sure that the dependencies are correctly defined (packages and version number).
2. In the `push.bat` and `AssembyInfo.cs` files, modify the version number according to [semantic versioning](https://semver.org/).
3. Build the solution.
4. Run `push.bat`.
5. In the `push.log` file, check that everything went OK.
6. Check in the modifications with a comment stating the version number and the reason (for example “2.2.4 bug fix”).


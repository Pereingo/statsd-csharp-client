Thanks for thinking about contributing something to the project! All suggestions, bugs, even typo fixes are most appreciated.

Please open an issue and have a chat about any features you're thinking of implementing before you start, so we can discuss the best way to go about it.

## Purpose

Our aim when making this library was to keep it small, lightweight, and very easy to configure and use. We'd rather not have more complex features that'd bloat its size or complexity and are usually just for edgecases or much higher scale. We're also in favour of statics where possible for that ease of use with the idea that you should (almost) never need to test the values you're sending for metrics - they should be incidental to the code and not run during tests.

Others have implemented features like this, such as the [high performance](https://github.com/Kyle2123/statsd-csharp-client) fork which does in-memory batching, or [StatsN](https://github.com/TryStatsN/StatsN) which skips statics in favour of testability.

## Running

Clone out the repo, fire up with Visual Studio 2015+, and ideally use ReSharper to run all the tests. If you don't have ReSharper, no worries, just grab the matching NUnit binary and either run it from the command line or use its GUI.

## Deploying

* Change major/minor versions in `appveyor.yml` if needed (build number is handled by AppVeyor)
* Update `CHANGELOG.md` to note expected build number after AppVeyor runs
* The NuGet package is generated as an artefact on AppVeyor. Grab that `*.nupkg` and upload it to NuGet.org.
* Create a git tag in the `v1.2.3` format

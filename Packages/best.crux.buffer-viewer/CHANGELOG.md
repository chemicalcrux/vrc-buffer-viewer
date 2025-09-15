# Changelog

All notable changes to this project will be documented in this file.

A changelog wasn't kept until version 0.2.2, so the changelog is currently being filled in.

Releases prior to 0.2.0 were deleted, as they used a different package ID. This caused duplicate entries to appear in the VCC.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.4] - 2025-09-15

### Fixed

- Switching scenes would break the stencil viewer

## [0.2.3] - 2025-09-15

### Fixed

- Stop spamming the logs about the near/far clip distances
- The LICENSE and README files did not have .meta files
  - This is probably a non-issue, but it's for the sake of consistency.

## [0.2.2] - 2025-09-15

### Added

- Documentation and changelog URLs for the package manifest

### Changed

- The package is now MIT-licensed

## [0.2.1] - 2025-09-09

### Changed

- Renamed some scripts to fit the new "Buffer Viewer" name

### Fixed

- _StencilComp caused warnings because it was of type Integer, rather than Float

## [0.2.0] - 2025-09-07

### Added

- A dependency on the Core package
- A choice of stencil comparison operators
- Depth-buffer viewing
- A render-queue timeline to show where things are being rendered
- An option to use Multiply blending
  - Normally, the visualized buffer completely covers up the original image

### Changed

- **BREAKING:** rename the package from "Stencil Viewer" to "Buffer Viewer" and changed the package name
- Store settings in a ScriptableSingleton
  - This prevents them from getting lost during domain reloads
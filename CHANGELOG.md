# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-exp.1] - 2021-04-06

This is the first release of the *AI Navigation* package. It contains the scripts that were previously known as *NavMeshComponents* and it adds a few improvements.

### Fixed
* Disabling a NavMeshLink component in the Editor does not remove the link

### Added
* New `minRegionArea` property in `NavMeshSurface` that prevents small isolated patches from being built in the NavMesh
* Documentation for the new `minRegionArea` property

### Changed
* Documentation updated
* The [license](LICENSE.md) has changed.
* The folder structure has changed in accordance to the requirements of the Unity standards for packages.

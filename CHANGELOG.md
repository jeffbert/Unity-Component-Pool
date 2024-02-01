# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.8] - 2024-02-01

### Changes

- ComponentPool.GetMany List parameter type changed to ICollection.

## [1.0.7] - 2024-01-29

### Changes

- Added preprocessor directives for unit tests.

## [1.0.6] - 2024-01-23

### Fixed

- Fixed objects not getting unpooled when they were reactivated without going through the component pool API (enabling component or activating game object).

## [1.0.3] - 2022-11-21

### Fixed

- Fixed README image urls
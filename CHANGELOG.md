# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Unity 7 support: pools are cleared when entering Play mode without a domain reload (`[OnEnteringPlayMode]` in Unity 7, `RuntimeInitializeOnLoadMethod` before).

### Changes

- A source's instances are shared regardless of the type argument used to get them (e.g. `Get<MonoBehaviour>(source)` and `Get(source)`), and with the `ComponentPoolInitializer`.
- `ComponentPool.DestroyInstances<T>` also destroys instances of sources whose type derives from `T`.
- `ComponentPool.CreateInstances` no longer uses reflection.
- Minimum Unity version is now 2021.3 (the package requires C# 9 since 1.0.11).

### Fixed

- Fixed the same instance being returned multiple times when getting instances under an inactive parent.
- Fixed the pool getting corrupted (and an `ArgumentOutOfRangeException`) when an instance created under an inactive parent was activated.
- Fixed `ComponentPool.CreateInstances` (and `ComponentPoolInitializer`) adding active instances to the pool when the pool wasn't empty.
- Fixed reused instances keeping the lossy scale of their previous parent.
- Fixed `ComponentPool.DestroySourceInstances`/`DestroyInstances` throwing when an instance was destroyed before ever being active.
- Fixed the `ComponentPoolInitializer` inspector creating textures on every GUI event, and its button tooltip not showing.

## [1.0.11] - 2026-06-18

### Changes

Unity 6.5 update

## [1.0.9] - 2024-02-11

### Fixed

- Fixed objects not getting unpooled when they were reactivated without going through the component pool API (enabling component or activating game object).

## [1.0.8] - 2024-02-01

### Changes

- ComponentPool.GetMany List parameter type changed to ICollection.

## [1.0.7] - 2024-01-29

### Changes

- Added preprocessor directives for unit tests.

## [1.0.3] - 2022-11-21

### Fixed

- Fixed README image urls
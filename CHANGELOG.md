# Changelog

<!-- There is always Unreleased section on the top. Subsections (Add, Changed, Fix, Removed) should be Add as needed. -->
## Unreleased

## 1.6.0 - 2019-06-26
- Add lint

## 1.5.0 - 2019-06-12
- Allow `Service` identification to graylog additional options

## 1.4.0 - 2019-06-06
- Add `Graylog.Diagnostics` module
    - Add `isAlive` function to allow checking Graylog status

## 1.3.0 - 2019-06-05
- Expose functions for getting a current verbosity level

## 1.2.0 - 2019-06-05
- Change Graylog message
    - Use `Environment.MachineName` as `source` instead of `facility`
    - Use `Facility` as `facility`

## 1.1.0 - 2019-06-05
- Allow logging to Graylog

## 1.0.0 - 2019-01-31
- Initial implementation

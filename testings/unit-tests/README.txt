The test project must be built with NUnit 2.4.3 or a previous version.
NUnit 2.4.4 and later versions use the log4net library as does LANDIS-II.
But NUnit 2.4.4+ uses log4net 1.2.10 while LANDIS-II 5.1 includes
log4net 1.2.9, which results in an assembly load error when attempting
to run the tests.
Setup required before you can run the unit tests
1. Install sql server if none exists
2. Create login "siewhooi", password "siewhooi"
3. Run sandbox.sql to create a sandbox database
4. Create a DSN, name=sandbox that points to the sandbox database
5. Update the _dataSource variable in SqlServerHandlerTests.cs to point to your sql server instance



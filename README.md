# Send Email Using .NET (Liara)
## Steps
```
git clone https://github.com/liara-cloud/dotnet-getting-started.git
```
```
cd dotnet-getting-started
```
```
git checkout email-server
```
```
dotnet restore # if needed
```
```
mv .env.example .env # and set ENVs
```
```
dotnet run
```
- check `http://localhost:5132/send-test-email`

## Need more INFO?
- [Liara Docs](https://docs.liara.ir/email-server/how-tos/connect-via-platform/dotnet/)
- [Dotnet Mail Docs](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail?view=net-8.0)

# Introduction 
A simple mailer API utilized by the Website project to send emails.

# Getting Started
Run the solution in debug mode to open the Swagger UI.

# Consume the API
A deployed instance of the API can be consumed via HTTP requests.

```
curl -X 'POST' \
  'https://localhost:6001/Email' \
  -H 'accept: */*' \
  -H 'X-Auth-ApiKey: CE427A72-3964-4409-8D12-D923D1F202C7' \
  -H 'Content-Type: multipart/form-data' \
  -F 'To=test1@example.com' \
  -F 'To=test2@example.com' \
  -F 'Subject=Test Mail' \
  -F 'Body=Hello World' \
  -F 'Attachments=@SomeDocument.pdf;type=application/pdf'
```

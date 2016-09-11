# Shimenawa API Details
This document will provide endpoints available inside Shimenawa

## Contents
- [Authentication](#qathentication)
- [APIs](#apis)
    -  [Root](#root)
    -  [Create Request](#create-request)
    -  [Get Request](#get-request)
    -  [Get Logs](#get-logs)
- [Notification](#notification)
    -  [Get Notification](#callback-notification)

## Authentication
None (Left blank intentionally for flexibility)

## APIs

### Root
Gets root document describing major api endpoints/links

##### Operation
`GET /api/v1`

##### Request
no parameters

##### Response
Returns a root document.

Content type: `application/hal+json`  

###### Response sample
```json
{
  "_links": {
    "self": {
      "href": "http://localhost:5311/api/v1"
    },
    "create_request": {
      "href": "http://localhost:5311/api/v1/requests",
      "title": "Create Sumo Log Search Request",
      "type": "application/json"
    },
    "request_id": {
      "href": "http://localhost:5311/api/v1/requests{?request_uuid}",
      "templated": true
    }
  }
}
```

### Create Request

### Get Request

### Get Logs

## Notification

### Callback Notification

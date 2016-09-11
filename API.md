# Shimenawa API Details
This document will provide endpoints available inside Shimenawa

## Contents
- [Authentication](#athentication)
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
no request parameters required

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
Creates a Sumo Log Search Request

##### Operation
`POST /api/v1/requests`

##### Request
Request body required
    - `query` : sumo logic query string
	- `from` : start date time to analyze. The format should be ISO 8601.
	- `to` : end date time to analyze. The format should be ISO 8601.
	- `callback_endpoint`(optional) : POST api endpoint to receive notification if request search is finished.

Content type: `application/json`  

##### Request Sample
```json
{
  "query": "b490ab294b0f0838",
  "from": "2016-09-06T00:00:10Z",
  "to": "2016-09-07T23:59:59Z",
  "callback_endpoint" : "http://abc.xyz.com/api/v1/request_search"
}
```

##### Response
Returns a HAL document that contains the request uuid and request parameters

Content type: `application/hal+json`  

###### Response sample
```json
{
  "request_uuid": "00000000-0000-0000-0000-000000000012",
  "query": "b490ab294b0f0838",
  "from": "2016-09-06T00:00:10Z",
  "to": "2016-09-07T23:59:59Z",
  "callback_endpoint" : "http://abc.xyz.com/api/v1/request_search",
  "_links": {
    "self": {
      "href": "http://localhost:5311/api/v1/requests/00000000-0000-0000-0000-000000000012"
    }
  }
}
```

### Get Request

### Get Logs

## Notification

### Callback Notification

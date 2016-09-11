# Shimenawa
Shimenawa is a web application that manages search queries to Sumo Logic to get target logs and deliver to requesters/consumers effectively and efficiently.
It uses the [Sumo Search Job API](https://help.sumologic.com/APIs/02Search_Job_API/About_the_Search_Job_API) to fetch logs from Sumo.

## API
[API document](API.md)

## How to setup and run locally
1. Set the connection string value inside `connectionStrings.config`
2. Set the values of confguration keys inside appsettings.config. Check out the following description per key.

| Key   										| Description 	            |SampleValue 	|
|---											|---			            |---			|
| BaseUri										| Shimenawa Base Url	    | http://localhost:5311/ |
| SumoApiUri									| Sumo API service endpoint | https://api.sumologic.com/api/v1/ |
| SumoAccessId									| Sumo Access Id            | 				|
| SumoAccessKey									| Sumo Access Key			|				|
| SumoLogWaitTimeBeforeStartSearch				| Allowance time before starting search (after to value) |	60000 |
| SumoLogIntervalWaitTimeBeforeSearchQueryMs    | Interval time before querying again to SUMO API  			    | 60000	|
| SumoApiRequestRateLimit						| Limits Sumo API request at a time to prevent error 429  | 4 |
| HangfireWorkerCount							| Hangfire process count | 10 			|

3. Run through Visual Studio
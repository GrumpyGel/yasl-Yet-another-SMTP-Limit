# yasl-Yet-another-SMTP-Limit
An SMTP message limiter for hMailServer


[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit">
    <img src="source/images/SudokuScreen_2.png" alt="Logo" width="180">
  </a>

  <h3 align="center">yasl-Yet-another-SMTP-Limit</h3>

  <p align="center">
    Alternative to httpWebRequest allowing modern ciphers on versions of Windows Server prior to 2022
    <br />
    <br />
    <br />
    <a href="http://www.mydocz.com/mdzWebRequest_Test.aspx">View Demo</a>
    ·
    <a href="https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/issues">Report Bug</a>
    ·
    <a href="https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/issues">Request Feature</a>
  </p>
</p>



<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary><h2 style="display: inline-block">Table of Contents</h2></summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#installation--usage">Installation &amp; Usage</a></li>
    <li><a href="#documentation">Documentation</a></li>
    <li><a href="#documentation">Security</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

A site I manage integrates to a 3rd Party that updated their SSL connectivity and in doing so restricted the number of allowable ciphers for clients to connect with to 3 (for TLS 1.2).  They are as follows:

TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256<br />
TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384<br />
TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256 

The managed site operates in a .Net environment and its httpWebRequest classes use the underlying operating system's https commmunication facilities. The 3 ciphers were not supported on anything other than Windows Server 2022 and we were not in a position to migrate to this platform.

I therefore investigated placing another server in our network to operate as a 'proxy' for our calls. The 3rd party uses Linux and PHP, so this was a natural choice.

However, we have PHP installed on our Windows Servers, so investigated if curl within PHP would use these ciphers in a Windows environment.

They did indeed make use of these ciphers, so I have put together a mdzWebRequest package of PHP proxy and .Net class to wrap the .Net httpWebRequest class to optionally make use of the proxy.

<!-- GETTING STARTED -->

## Installation & Usage

Clone the repo
   ```sh
   git clone https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit.git
   ```


<!-- DOCUMENTATION -->
## Documentation

To make a web request using mdzWebRequest perform the following:

<ol>
<li>Create an instance of mdzWebRequest, for example MyRequest = new mdzWebRequest();</li>
<li>Set the Request Properties</li>
<li>Envoke the Submit() Method</li>
<li>Check the Response Properties</li>
</ol>

### Request Properties

| Property | DataType | Description |
| --- | --- | --- |
| URL | string | The URL for the service you wish to request |
| Method | string | The request method, must be "GET", "POST" or "PUT". |
| Content | string | Any data to post |
| ContentType | string | Mime type for data to be posted, fopr example "application/x-www-form-urlencoded", "text/xml; encoding='utf-8'" |
| UserName | string | If authentification is required, the UserName |
| Password | string | If authentification is required, the Password |
| ExpectedFormat | string | The response can be returned as a string or binary (btye[]), see below for options |
| MaxBinarySize | int | If the response is Binary, this is the maximum allowable size |
| UseProxy | bool | If false, the request will be made using a httpWebRequest object, if True the request will be made via the mdzWebRequest_Proxy.php |
| ProxyURL | string | URL to access to proxy. |
| ProxyUserName | string | If authentification is required to access the proxy, the UserName |
| ProxyPassword | string | If authentification is required to access the proxy, the Password |

#### Expectedformat

The ExpectedFormat property may be set to one of the following:

| Value | Description |
| --- | --- |
| Text | The response is expected to be Text and will be returned in the Response property as a string, only use when safe to do so |
| Binary | The response is expected to be Binary and will be returned in the ResponseBinary property as a byte[] |
| Detect (Default) | When ResponseType is "text/*", "application/xhtml+xml", "application/xml" or "application/json" it will be processed as Text, otherwise it will be processed as Binary |

### Response Properties

| Property | DataType | Description |
| --- | --- | --- |
| ErrNo | int | An error code that may be from mdzWebRequest or culr if using the proxy, see below |
| ErrMsg | string | An error code that may be from mdzWebRequest or culr if using the proxy, see below |
| ResponseCode | HttpStatusCode | The response status code returned by the server - see [https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode) |
| ResponseType | string | The response Mine Content Type.  Any parameters following the Content type in the header supplied by the server are stripped, for example "text/html; charset=utf-8" will return just "text/html" |
| ResponseTypeParams | string | Any parameters following the Content Type, for example "text/html; charset=utf-8" will return "charset=utf-8" |
| IsBinary | bool | If True, the response has been treated as Binary and is therefore provided in the ResponseBinary property.  If False the response is treated as Text and is provided in the Response property |
| Response | string | The response data returned by the server, if the IsBinary property is False |
| ResponseBinary | byte[] | The response data returned by the server, if the IsBinary property is True |

### Error Handling

If the ErrNo property has a value of 0 after Submit() has been envoked, the requested was processed successfully.

This does not necessarily mean that the resource requested performed correctly, the ResponseCode property should also be checked for OK/200 and any other logic associated with the request performed on the Response(/Binary) data.

For direct (non-proxy) requests, other than the first 2 errors listed below, ErrNo will always return 0.  If an exception is thrown by the httpWebRequest wrapper, these are thrown back to the calling program.

For proxy requests, exceptions should be less likely as they are trapped and return the 14011 and 14013 codes listed below.  Although this may not give such high detail on the actual error, it highlights what part of the process failed and the exact message is still returned.  If an error was returned by the curl request to the server, this is returned in ErrNo.

| ErrNo | Description |
| --- | --- |
| 14001 | The URL property is blank |
| 14002 | The Method property is not "GET", "POST" or "PUT" |
| 14003 | The UseProxy property is True, but ProxyURL property is blank |
| 14004 | The request was not allowed, client IP or Host blocked , see Configuration below.  This error will also be raised if you have Host exceptionsand the supplied URL property is invalid meaning the Host could not be extracted from it. |
| 14011 | A proxy request was made, but an error was thrown communicating with the proxy, ErrMsg will include a description |
| 14012 | The ResponseCode received from the proxy request was not 200 indicating failure, ErrMsg will include the ResponseCode  |
| 14013 | An error occurred extracting the request response from the proxy response, ErrMsg will include a description |

A list of curl ErrNo codes can be found at https://curl.se/libcurl/c/libcurl-errors.html

When exceptions are raised within the mdzWebRequest class, even if they are not passed on as exception but return 14011 and 14013 error codes, the mdzSys.ErrorLog() function is called.  This can be configured to email details of the error raised and log them to a file.  These are configured within the <smpt> and <errorlog> sections of the mdzSys.config file and documented in the mdzSys.cs source.
  
### Configuration

mdzWebRequest allows for validation of allowable client IP addresses (ie browser or service client IP) and Host server names in the URL property.  The following settings are available:

| Setting | Description |
| --- | --- |
| IP_AutoAllow | A 'True' value means all client IP addresses are by default allowed, a 'False' value means no IP addresses are by default allowed |
| Host_AutoAllow | A 'True' value means all hosts are by default allowed, a 'False' value means no hosts are by default allowed |
| ValidationLog | If a log is required of validation failures, the log filename should be set here.  If the name is prefixed by a '~' character it will be created in the site directory.  If empty, no log is produced. |
| Exception | Exceptions to the IP_AutoAllow and Host_AutoAllow settings can each be made as a separate Exception.  The Exception should have a "Type" attribute of "IP" or "Host" and a "Value" attribute of the IP address/Host name that is the exception.  IP address values can be IPV4 or IPV6 and can optionally include CIDR notation for subnet mask.  IPV6 code has not been tested. |
  
Configuration settings are made in the mdzWebRequest.config file which is in XML format.  The following example only allows client connection from 127.0.0.1 and 192.168.1.* IP addresses, only allows requests to be made to www.mydocz.com and will log validation failures in a file called 'mdzWebRequest.log':
  
```
<mdzWebRequest IP_AutoAllow="False"
               Host_AutoAllow="False"
               ValidationLog="~mdzWebRequest.log">
    <Exception Type="IP"   Value="127.0.0.1"/>
    <Exception Type="IP"   Value="192.168.1.1/24"/>
    <Exception Type="Host" Value="www.mydocz.com"/>
</mdzWebRequest>
```  

Configuration is loaded by a singleton mdzWebRequestConfig class (defined in mdzWebRequest.cs).  This is loaded the first time mdzWebRequest is referenced and therefore improves performance as it does not need to be parsed for each use of mdzWebRequest.  However, once loaded it does not reload the configuration file.  Therefore if the configuration file is edited, the web site should be restarted to reload the mdzWebRequestConfig class.

### Source Files

The files comprising mdzWebRequest are as follows:
  
| Filename | Description |
| --- | --- |
| APP_CODE/mdzWebRequest.cs | The mdzWebRequest class written in c# |
| APP_CODE/mdzSys.cs | A static singleton class with various helper functions used by MyDocz code including mdzWebServices.cs |
| mdzWebRequestProxy.php | The mdzWebRequest proxy PHP program |
| mdzWebRequest_Test.aspx | A program to test mdzWebRequest as used on the MyDocz web site |
| mdzWebRequest_Test.xslt | The mdzWebRequest_Test pages HTML as a XSLT stylesheet |
| mdzWebRequest_Test.css | CSS stylesheet used by mdzWebRequest_Test pages |
| mdzWebRequest_Test.js | Javascript file used by mdzWebRequest_Test pages |
| mdzWebRequest.config | Sample configuration file for mdzWebRequest (see Configuration above) |
| mdzSys.config | Configuration file for mdzSys functions (see Error Handling above) |

<!-- SECURITY -->
## Security

 Although the mdzWebRequest class has configuration to filter IP addresses and hosts being connected to, the proxy component does not.
 
 As mdzWebRequest_Proxy.php will forward all web requests it receives, it opens up 'relay' type security issues.  It currently has no facility to deny usage based on client IP or any other criteria.

It should therefore not be installed on a publicly addressable server.

It can though be hosted on the same server as the public site, but under a different web site.  The MyDocz site hosts mdzWebRequest.php on a separate site under the domain name of mdzwr.mydocz.com.  Normally, this site would be configured so that it only allows connection from trusted IP addresses, for example IP addresses within the local network (or loopback) where mdzWebRequest is running and under the control of the application using it.

On the MyDocz site though the www.mydocz.com/mdzWebRequest_Test.aspx test page operates, which itself could be abused as a relay.  So simply restricting access to mdzwr.mydocz.com from local IPs would be pointless.  Instead the mdzwr.mydocz.com domain is also configured to only allow 2 requests per 10 seconds.  This is fine to stop abuse, and no live applications use this domain, but would not be practical in a normal live environment.


<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Email - [grumpygel@mydocz.com](mailto:grumpygel@mydocz.com)

Project Link: [https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit](https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit)



<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements

* [Best-README-Template](https://github.com/othneildrew/Best-README-Template)
* [IP and subnet validation code adapted from Christoph Sonntag's Stack Overflow thread response](https://stackoverflow.com/a/56461160)




<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/GrumpyGel/yasl-Yet-another-SMTP-Limit.svg?style=for-the-badge
[contributors-url]: https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/GrumpyGel/yasl-Yet-another-SMTP-Limit.svg?style=for-the-badge
[forks-url]: https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/network/members
[stars-shield]: https://img.shields.io/github/stars/GrumpyGel/yasl-Yet-another-SMTP-Limit.svg?style=for-the-badge
[stars-url]: https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/stargazers
[issues-shield]: https://img.shields.io/github/issues/GrumpyGel/yasl-Yet-another-SMTP-Limit.svg?style=for-the-badge
[issues-url]: https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/issues
[license-shield]: https://img.shields.io/github/license/GrumpyGel/yasl-Yet-another-SMTP-Limit.svg?style=for-the-badge
[license-url]: https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/gerald-moull-41b5265

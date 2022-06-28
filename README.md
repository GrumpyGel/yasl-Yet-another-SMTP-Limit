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
    <img src="yasl_ShowSent.png" alt="Sent Summary">
  </a>

  <h3 align="center">yasl-Yet-another-SMTP-Limit</h3>

  <p align="center">
    This is a module to apply hourly and daily limits to the number of messages
hMailServer accounts can send.  This was created to stop hacked accounts being
used to send spam (or at least very much limit it).
    <br />
    <br />
    <br />
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
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

I did someone a favour and said I'd host their email on my hMailServer.

I set the domain and accounts up that they requested, and being a rather naive email administrator, set them up with simple passwords asking them to log in immediately to set up decent secure ones.

Of course they didn't. So even though I'd set external IPs up as needing authentication, the result was that their accounts were hacked and all of a sudden my server was spewing out spam. grrr.

I went in and put strong passwords on all the accounts - but there's nothing to stop them putting simple ones back and I don't really know what else might compromise the system. So searched on the hMailServer forum for something to try and stop this happening again - and found the SmtpLimit utility. Perfect.

Well it would be if it wasn't reliant on reading through lots of file data for each message. When the spammers were hitting my server the server had trouble enough keeping up, adding this file overhead I'm sure would bring the server down - granted it would stop the spam issue, but it would also compromise other services. So I looked at how SmtpLimit interacted with hMailServer and wrote Yet Another Smtp Limit (yasl) which works in memory. It is written in c# and is dependent on and runs under IIS with a VBScript client loaded by hMailServer - I wasn't sure if/how I could persist the data within the VBScript environment.

It runs on set Daily/Hourly limits that can be set per account and have time based overrides - eg if someone is going to do a mail shot. It does save hourly logs, so a process could be created to analyse these like SmtpLimit does and make appropriate Config settings. However, I only host a few accounts - so stopping spam is my priority, I don't mind a bit of admin every now and then to set the config. I've also added a configurable feature to disable the sending of email if the From address is not the same as the authorised login account. There's a couple of admin pages to show the config setup (for example, to ensure it is loaded correctly) and to view which accounts have sent email and how many emails they have sent - highlighted if they are near their limit (a warning email is sent like in SmtpLimit) or over it.

<!-- GETTING STARTED -->

## Installation & Usage

Clone the repo
   ```sh
   git clone https://github.com/GrumpyGel/yasl-Yet-another-SMTP-Limit.git
   ```


<!-- DOCUMENTATION -->
## Documentation

The 'yasl' class performs limiting logic/processing.  It is a 'singleton' class/object written in c#.  Being a singleton, rather than static, object creation and destruction is controlled to enable current data to be saved (on destruction) and reloaded (on creation) so that its state is maintained.  The configuration is also loaded on the class creation.

Daily limits do not relate to a calendar day, they cover activity in the previous 24 hours.  Hourly limits relate to each hour, not the previous 60 minutes.

The yasl class is front-ended by a yasl.aspx URL.  The vbscript code loaded by hMailServer extracts the required information from the Client and Message parameters to the onAcceptMessage called by hMailServer and posts these to the yasl.aspx URL.  The response determines if the client will instruct hMailServer to send the message or not.

There is also a yasl_Admin.aspx URL that is viewed through a browser to show the current state on the yasl singleton - ie config and accounts that have sent mail.  It can also be used to reload changed config files rather than destroying the object.

yasl can also be used to ensure that all email sent by and account is has a matching from address.

### Server

The Server folder contains content that should be extracted to an IIS website folder which will host it.

The yasl code has no security to ensure outsiders are not interfering with it. The IIS site that hosts should be configured to only allow trusted IP addresses - ie the mail server and administrative/testing clients and servers.

The yasl class is found in the yasl.cs file in the APP_CODE folder.  There is limited documentation at class and helper class level within this file.

### Client

The Client folder content should be placed into the Events folder of the hMailServer installation.  As well as the EventHandlers.vbs file used by hMailServer, there is also a yasl_client_test.vbs script that can be used to test connection to the server and the server itself.  Command line usage for this is found in the file.

### Configuration

The yasl class is configured using the yasl_Config.xml and yasl_Overrides.xml files.  These are documented in the yasl class in the APP_CODE\yasl.cs file.

The client should be configured in the EventHandlers.vbs file.  It is documented in the file what can be set - eg admin email address for warning/error messages.


### Source Files

The files comprising mdzWebRequest are as follows:
  
| Filename | Description |
| --- | --- |
| Source/APP_CODE/yasl.cs | The singleton class performing all messsage validation |
| Source/yasl.aspx | The page hit by the client.  It extracts the request information (eg sending address and number of copies), passes this the the yasl class for validation and returns the response to the client |
| Source/yasl_Admin.aspx | Administration pages that can be used to view current configuration and activity and reload configuration |
| Source/yasl_Routines.aspc | Routines used by the above pages |
| Source/yasl_ShowConfig.xslt | The Admin page showing configuration |
| Source/yasl_ShowSent.xslt | The Admin page showing activity |
| Source/yasl_Include.xslt | The page template for Admin pages |
| Source/yasl.css | CSS stylesheet for Admin pages |
| Source/yasl_Config.xml | Sample configuration file for yasl |
| Source/yasl_Overrides.xml | Sample configuration time based overrides file for yasl |
| Client/EventHandlers.vbs | Client plug in to hMailServer |
| Client/yasl_client_test.vbs | VB Script testing client and server. |



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
* [Palinka's "Limit Outgoing Messages" project on hMailServer forum](https://hmailserver.com/forum/viewtopic.php?t=34882)




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

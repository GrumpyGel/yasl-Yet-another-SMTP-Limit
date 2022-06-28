<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="html" indent="yes"/>

<xsl:variable name="ver">ver=2</xsl:variable>

<xsl:template name="yasl_Page">
  <xsl:param name="title"></xsl:param>
  <html>
    <head>
      <title>yasl Admin<xsl:if test="$title != ''">&#160;-&#160;<xsl:value-of select="$title"/></xsl:if></title>
      <meta http-equiv="Content-Type" content="text/html" charset="utf-8"/>
      <meta charset="utf-8"/>
      <link rel="stylesheet" type="text/css" href="/yasl.css?{$ver}"/>
    </head>
    <body>
      <div class="Header">
        <h1>yasl Admin - <xsl:value-of select="$title"/></h1>
        <xsl:choose>
          <xsl:when test="@ResultStatus = 'OK'">
            <table class="rowey">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Hour</th>
                  <th>Limit Hour</th>
                  <th>Qty Static</th>
                  <th>Qty Temp</th>
                  <th>Qty Accounts</th>
                  <th>Qty Errors</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td><xsl:value-of select="@CurrDate"/></td>
                  <td><xsl:value-of select="@CurrHourDisp"/></td>
                  <td><xsl:value-of select="@LimitHour"/></td>
                  <td><xsl:value-of select="@QtyStatic"/></td>
                  <td><xsl:value-of select="@QtyTemp"/></td>
                  <td><xsl:value-of select="@QtySent"/></td>
                  <td><xsl:value-of select="@QtyErrors"/></td>
                </tr>
              </tbody>
            </table>
            <xsl:if test="Errors/Error">
              <div class="Errors">
                <ul>
                  <xsl:for-each select="Errors/Error">
                    <li><xsl:value-of select="."/></li>
                  </xsl:for-each>
                </ul>
              </div>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <h3>Error Loading Server Status : <xsl:value-of select="@ResultStatus"/></h3>
          </xsl:otherwise>
        </xsl:choose>
        <div class="Actions">
          <a href="yasl_Admin.aspx?Action=showconfig">Show Config</a>
          <a href="yasl_Admin.aspx?Action=loadconfig">Reload Config</a>
          <a href="yasl_Admin.aspx?Action=showsent">Show Account Status</a>
          <a href="yasl_Admin.aspx?Action=savesent">Save Account Status</a>
          <a href="yasl_Admin.aspx?Action=clearerrors">Clear Errors</a>
        </div>
      </div>
      <div class="Content">
        <xsl:call-template name="my_Content"/>
      </div>
    </body>
  </html>
</xsl:template>


</xsl:stylesheet>
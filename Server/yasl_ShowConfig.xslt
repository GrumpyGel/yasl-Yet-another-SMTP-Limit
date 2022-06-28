<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="html" indent="yes"
            doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
            doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"/>


<xsl:template match="/yasl">
  <xsl:call-template name="yasl_Page">
    <xsl:with-param name="title" select="'Show Config'"/>
  </xsl:call-template>
</xsl:template>


<xsl:template name="my_Content">
  <xsl:if test="@ResultLoad">
    <h3><xsl:choose><xsl:when test="@ResultLoad = 'OK'">Config Reloaded OK</xsl:when><xsl:otherwise>Error Loading Config : <xsl:value-of select="@ResultLoad"/></xsl:otherwise></xsl:choose></h3>
  </xsl:if>
  <xsl:if test="@ResultClearErrors">
    <h3><xsl:choose><xsl:when test="@ResultClearErrors = 'OK'">Errors Cleared OK</xsl:when><xsl:otherwise>Error Clearing Errors : <xsl:value-of select="@ResultClearErrors"/></xsl:otherwise></xsl:choose></h3>
  </xsl:if>
  <xsl:if test="@Result != 'OK'">
    <h3>Error Retrieving Config : <xsl:value-of select="@Result"/></h3>
  </xsl:if>
  <h3>Defaults:</h3>
  <table class="columney">
    <tbody>
      <tr><th>Limit Daily</th><td><xsl:value-of select="Config/@Limit_Day"/></td></tr>
      <tr><th>Limit Hourly</th><td><xsl:value-of select="Config/@Limit_Hour"/></td></tr>
      <tr><th>Limit Warn</th><td><xsl:value-of select="Config/@Limit_Warn"/>&#160;&#160;&#160;%</td></tr>
      <tr><th>Limit Warn Clear</th><td><xsl:value-of select="Config/@Limit_Warn_Clear"/>&#160;&#160;&#160;%</td></tr>
      <tr><th>Retain Ignore</th><td><xsl:value-of select="Config/@Retain_Ignore"/></td></tr>
      <tr><th>Retain Include</th><td><xsl:value-of select="Config/@Retain_Include"/>&#160;&#160;&#160;%</td></tr>
      <tr><th>Allow Diff From</th><td><xsl:value-of select="Config/@Allow_Diff_From"/></td></tr>
      <tr><th>Max Errors</th><td><xsl:value-of select="Config/@Max_Errors"/></td></tr>
    </tbody>
  </table>
  <h3>Static Overrides:</h3>
  <table class="rowey">
    <thead>
      <tr>
        <th>Address</th>
        <th>Limit Day</th>
        <th>Limit Hour</th>
        <th>Allow Diff From</th>
      </tr>
    </thead>
    <tbody>
      <xsl:for-each select="Config/Static/Override">
        <tr>
          <td><xsl:value-of select="@Address"/></td>
          <td><xsl:value-of select="./@Limit_Day[. != /yasl/Config/@Limit_Day]"/></td>
          <td><xsl:value-of select="./@Limit_Hour[. != /yasl/Config/@Limit_Hour]"/></td>
          <td><xsl:value-of select="./@Allow_Diff_From[. != /yasl/Config/@Allow_Diff_From]"/></td>
        </tr>
      </xsl:for-each>
    </tbody>
  </table>
  <h3>Time Overrides:</h3>
  <table class="rowey">
    <thead>
      <tr>
        <th>Address</th>
        <th>Time Start</th>
        <th>Time End</th>
        <th>Limit Day</th>
        <th>Limit Hour</th>
        <th>Allow Diff From</th>
      </tr>
    </thead>
    <tbody>
      <xsl:for-each select="Config/Temp/Override">
        <tr>
          <td><xsl:value-of select="@Address"/></td>
          <td><xsl:value-of select="@TimeStart"/></td>
          <td><xsl:value-of select="@TimeEnd"/></td>
          <td><xsl:value-of select="./@Limit_Day[. != /yasl/Config/@Limit_Day]"/></td>
          <td><xsl:value-of select="./@Limit_Hour[. != /yasl/Config/@Limit_Day]"/></td>
          <td><xsl:value-of select="./@Allow_Diff_From[. != /yasl/Config/@Allow_Diff_From]"/></td>
        </tr>
      </xsl:for-each>
    </tbody>
  </table>
</xsl:template>


<xsl:include href="yasl_Include.xslt"/>

</xsl:stylesheet> 

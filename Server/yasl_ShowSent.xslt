<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="html" indent="yes"
            doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
            doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"/>


<xsl:template match="/yasl">
  <xsl:call-template name="yasl_Page">
    <xsl:with-param name="title" select="'Show Account Status'"/>
  </xsl:call-template>
</xsl:template>


<xsl:template name="my_Content">
  <xsl:variable name="QtyDom" select="count(Domains/Domain)"/>
  <xsl:if test="@ResultSave">
    <h3><xsl:choose><xsl:when test="@ResultSave = 'OK'">Account Status Saved OK</xsl:when><xsl:otherwise>Error Saving Account Status : <xsl:value-of select="@ResultSave"/></xsl:otherwise></xsl:choose></h3>
  </xsl:if>
  <xsl:if test="@Result != 'OK'">
    <h3>Error Retrieving Sent Status : <xsl:value-of select="@Result"/></h3>
  </xsl:if>
  <xsl:choose>
    <xsl:when test="$QtyDom = 0">
      <h3>No Sent Messages Recorded Today</h3>
    </xsl:when>
    <xsl:otherwise>
      <xsl:if test="$QtyDom != 1">
        <xsl:call-template name="SelectDomain"/>
      </xsl:if>
      <p>Show Sent Activity For Day <input type="checkbox" id="ShowAllDay" onchange="ShowAllDay();"/></p>
      <xsl:for-each select="Domains/Domain">
        <xsl:variable name="ID"><xsl:choose><xsl:when test="$QtyDom = 1">Only</xsl:when><xsl:when test="$QtyDom &gt; 10 and position() &gt; 9">Other</xsl:when><xsl:otherwise><xsl:value-of select="position()"/></xsl:otherwise></xsl:choose></xsl:variable>
        <div>
          <xsl:if test="$QtyDom != 1">
            <xsl:attribute name="class">domain_accounts</xsl:attribute>
            <xsl:attribute name="id">domain_accounts_<xsl:value-of select="position()"/></xsl:attribute>
          </xsl:if>
          <xsl:call-template name="ShowAccounts"/>
        </div>
      </xsl:for-each>
    </xsl:otherwise>
  </xsl:choose>
  <script language="javascript"><![CDATA[
    function ShowDomain(p_ID) {
      var i_Domains = document.getElementsByClassName("domain_accounts");
      var i_Sub = 0;
      
      for (i_Sub = 0; i_Sub < i_Domains.length; i_Sub++) {
        i_Domains[i_Sub].style.display = ((p_ID == 0 || i_Domains[i_Sub].id.substr(16) == p_ID) ? "block" : "none") ; }
    }
    function ShowAllDay() {
      if (document.getElementById("ShowAllDay").checked == true) {
        document.body.classList.add("ShowAllDay"); }
      else {
        document.body.classList.remove("ShowAllDay"); }
    }
    ]]></script>
</xsl:template>


<xsl:template name="ShowAccounts">
  <xsl:variable name="Domain" select="@Domain"/>
  <h3>Messages Sent and Effective Limits For <xsl:choose><xsl:when test="@Domain = ''">&lt;No Domain&gt;</xsl:when><xsl:otherwise>@<xsl:value-of select="@Domain"/></xsl:otherwise></xsl:choose>:</h3>
  <table class="rowey">
    <thead>
      <tr>
        <th>Address</th>
        <th>Retained</th>
        <th>Rejected</th>
        <th>Sent Day</th>
        <th>Limit Day</th>
        <th>Sent Hour</th>
        <th>Limit Hour</th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_22"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_21"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_20"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_19"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_18"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_17"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_16"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_15"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_14"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_13"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_12"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_11"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_10"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_9"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_8"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_7"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_6"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_5"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_4"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_3"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_2"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_1"/></th>
        <th class="AllDay"><xsl:value-of select="/yasl/Sent/@Hour_0"/></th>
      </tr>
    </thead>
    <tbody>
      <xsl:for-each select="/yasl/Sent/Account[@Domain = $Domain]">
        <tr>
          <td><xsl:value-of select="@Account"/></td>
          <td><xsl:if test="@Qty_Retained != 0"><xsl:value-of select="@Qty_Retained"/></xsl:if></td>
          <td><xsl:if test="@Qty_Rejected != 0"><xsl:value-of select="@Qty_Rejected"/></xsl:if></td>
          <td><xsl:choose><xsl:when test="@Over_Day = 'True'"><xsl:attribute name="class">Over</xsl:attribute></xsl:when><xsl:when test="@Warn_Day = 'True'"><xsl:attribute name="class">Warn</xsl:attribute></xsl:when></xsl:choose><xsl:value-of select="@Qty_Day"/></td>
          <td><xsl:value-of select="@Limit_Day"/></td>
          <td><xsl:choose><xsl:when test="@Over_Hour = 'True'"><xsl:attribute name="class">Over</xsl:attribute></xsl:when><xsl:when test="@Warn_Hour = 'True'"><xsl:attribute name="class">Warn</xsl:attribute></xsl:when></xsl:choose><xsl:value-of select="@Qty_Hour"/></td>
          <td><xsl:value-of select="@Limit_Hour"/></td>
          <td class="AllDay"><xsl:if test="@Earlier_22 != 0"><xsl:value-of select="@Earlier_22"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_21 != 0"><xsl:value-of select="@Earlier_21"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_20 != 0"><xsl:value-of select="@Earlier_20"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_19 != 0"><xsl:value-of select="@Earlier_19"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_18 != 0"><xsl:value-of select="@Earlier_18"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_17 != 0"><xsl:value-of select="@Earlier_17"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_16 != 0"><xsl:value-of select="@Earlier_16"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_15 != 0"><xsl:value-of select="@Earlier_15"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_14 != 0"><xsl:value-of select="@Earlier_14"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_13 != 0"><xsl:value-of select="@Earlier_13"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_12 != 0"><xsl:value-of select="@Earlier_12"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_11 != 0"><xsl:value-of select="@Earlier_11"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_10 != 0"><xsl:value-of select="@Earlier_10"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_9 != 0"><xsl:value-of select="@Earlier_9"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_8 != 0"><xsl:value-of select="@Earlier_8"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_7 != 0"><xsl:value-of select="@Earlier_7"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_6 != 0"><xsl:value-of select="@Earlier_6"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_5 != 0"><xsl:value-of select="@Earlier_5"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_4 != 0"><xsl:value-of select="@Earlier_4"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_3 != 0"><xsl:value-of select="@Earlier_3"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_2 != 0"><xsl:value-of select="@Earlier_2"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_1 != 0"><xsl:value-of select="@Earlier_1"/></xsl:if></td>
          <td class="AllDay"><xsl:if test="@Earlier_0 != 0"><xsl:value-of select="@Earlier_0"/></xsl:if></td>
        </tr>
      </xsl:for-each>
    </tbody>
  </table>
</xsl:template>


<xsl:template name="SelectDomain">
  <table class="columney">
    <tbody>
      <tr>
        <th>All Domains</th>
        <td><input type="radio" name="rad_domain" id="rad_all" onclick="ShowDomain(0);"/></td>
      </tr>
      <xsl:for-each select="Domains/Domain">
        <tr>
          <th><xsl:choose><xsl:when test="@Domain = ''">&lt;No Domain&gt;</xsl:when><xsl:otherwise>@<xsl:value-of select="@Domain"/></xsl:otherwise></xsl:choose></th>
          <td><input type="radio" name="rad_domain" id="rad_{position()}" onclick="ShowDomain({position()});"/> (<xsl:value-of select="@Qty"/>)</td>
        </tr>
      </xsl:for-each>
    </tbody>
  </table>
</xsl:template>


<xsl:include href="yasl_Include.xslt"/>

</xsl:stylesheet> 

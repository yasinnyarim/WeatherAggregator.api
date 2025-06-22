<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>

  <xsl:template match="/*[local-name()='WeatherReport']">
    <!-- Artık tam bir sayfa değil, sadece bir HTML parçası üretiyoruz -->
    <div class="container">
      <h1>
        Weather Report for <xsl:value-of select="City"/>
      </h1>
      <table class="report-table">
        <tr>
          <th>Parameter</th>
          <th>Value</th>
        </tr>
        <tr>
          <td>Temperature</td>
          <td>
            <xsl:value-of select="Temperature"/> °C
          </td>
        </tr>
        <tr>
          <td>Condition</td>
          <td>
            <xsl:value-of select="Condition"/>
          </td>
        </tr>
        <tr>
          <td>Humidity</td>
          <td>
            <xsl:value-of select="Humidity"/> %
          </td>
        </tr>
        <tr>
          <td>Wind Speed</td>
          <td>
            <xsl:value-of select="WindSpeed"/> m/s
          </td>
        </tr>
      </table>
      <p class="source-info">
        Data Source: <xsl:value-of select="Source"/>
      </p>
    </div>
  </xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>
  <xsl:template match="/*[local-name()='WeatherReport']">
    <div id="animation-layer">
      <div id="bg-default" class="active"></div>
      <div id="bg-temp"></div>
      <div id="bg-humidity"></div>
      <div id="bg-wind"></div>
      <div id="bg-condition"></div>
    </div>
    <div class="report-container">
      <h1>
        <i class="fas fa-map-marker-alt"></i>Weather Report for <xsl:value-of select="City"/>
      </h1>
      <table class="report-table">
        <tr id="row-temp">
          <td>
            <i class="fas fa-thermometer-half"></i> Temperature
          </td>
          <td>
            <span id="value-temp" data-value="{Temperature}">
              <xsl:value-of select="Temperature"/> °C
            </span>
          </td>
        </tr>
        <tr id="row-humidity">
          <td>
            <i class="fas fa-tint"></i> Humidity
          </td>
          <td>
            <span id="value-humidity" data-value="{Humidity}">
              <xsl:value-of select="Humidity"/> %
            </span>
          </td>
        </tr>
        <tr id="row-wind">
          <td>
            <i class="fas fa-wind"></i> Wind Speed
          </td>
          <td>
            <span id="value-wind" data-value="{WindSpeed}">
              <xsl:value-of select="WindSpeed"/> m/s
            </span>
          </td>
        </tr>
        <tr id="row-condition">
          <td>
            <i class="fas fa-cloud-sun-rain"></i> Condition
          </td>
          <td>
            <span id="value-condition" data-value="{Condition}">
              <xsl:value-of select="Condition"/>
            </span>
          </td>
        </tr>
      </table>
      <p class="source-info">
        Data Source: <xsl:value-of select="Source"/>
      </p>
    </div>
  </xsl:template>
</xsl:stylesheet>
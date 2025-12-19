namespace Altemiq.IO.Las;

public class WellKnownTextNodeTests
{
#if NET
    [Test]
    [Arguments("""
               PROJCRS["GDA2020 / Vicgrid",
               BASEGEOGCRS["GDA2020",
                   DATUM["Geocentric Datum of Australia 2020",
                       ELLIPSOID["GRS 1980",6378137,298.257222101,
                           LENGTHUNIT["metre",1]]],
                   PRIMEM["Greenwich",0,
                       ANGLEUNIT["degree",0.0174532925199433]],
                   ID["EPSG",7844]],
               CONVERSION["Vicgrid",
                   METHOD["Lambert Conic Conformal (2SP)",
                       ID["EPSG",9802]],
                   PARAMETER["Latitude of false origin",-37,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8821]],
                   PARAMETER["Longitude of false origin",145,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8822]],
                   PARAMETER["Latitude of 1st standard parallel",-36,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8823]],
                   PARAMETER["Latitude of 2nd standard parallel",-38,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8824]],
                   PARAMETER["Easting at false origin",2500000,
                       LENGTHUNIT["metre",1],
                       ID["EPSG",8826]],
                   PARAMETER["Northing at false origin",2500000,
                       LENGTHUNIT["metre",1],
                       ID["EPSG",8827]]],
               CS[Cartesian,2],
                   AXIS["(E)",east,
                       ORDER[1],
                       LENGTHUNIT["metre",1]],
                   AXIS["(N)",north,
                       ORDER[2],
                       LENGTHUNIT["metre",1]],
               USAGE[
                   SCOPE["State-wide spatial data management."],
                   AREA["Australia - Victoria."],
                   BBOX[-39.2,140.96,-33.98,150.04]],
               ID["EPSG",7899]]
               """)]
    public async Task ParseText(string wkt)
    {
        await Assert.That(WellKnownTextNode.Parse(wkt)).IsNotDefault();
    }
#endif

    [Test]
    [Arguments("""
               PROJCRS["GDA2020 / Vicgrid",
               BASEGEOGCRS["GDA2020",
                   DATUM["Geocentric Datum of Australia 2020",
                       ELLIPSOID["GRS 1980",6378137,298.257222101,
                           LENGTHUNIT["metre",1]]],
                   PRIMEM["Greenwich",0,
                       ANGLEUNIT["degree",0.0174532925199433]],
                   ID["EPSG",7844]],
               CONVERSION["Vicgrid",
                   METHOD["Lambert Conic Conformal (2SP)",
                       ID["EPSG",9802]],
                   PARAMETER["Latitude of false origin",-37,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8821]],
                   PARAMETER["Longitude of false origin",145,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8822]],
                   PARAMETER["Latitude of 1st standard parallel",-36,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8823]],
                   PARAMETER["Latitude of 2nd standard parallel",-38,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8824]],
                   PARAMETER["Easting at false origin",2500000,
                       LENGTHUNIT["metre",1],
                       ID["EPSG",8826]],
                   PARAMETER["Northing at false origin",2500000,
                       LENGTHUNIT["metre",1],
                       ID["EPSG",8827]]],
               CS[Cartesian,2],
                   AXIS["(E)",east,
                       ORDER[1],
                       LENGTHUNIT["metre",1]],
                   AXIS["(N)",north,
                       ORDER[2],
                       LENGTHUNIT["metre",1]],
               USAGE[
                   SCOPE["State-wide spatial data management."],
                   AREA["Australia - Victoria."],
                   BBOX[-39.2,140.96,-33.98,150.04]],
               ID["EPSG",7899]]
               """)]
    public async Task ParseBytes(string wkt)
    {
        var byteCount = System.Text.Encoding.UTF8.GetByteCount(wkt);
        Span<byte> bytes = stackalloc byte[byteCount];
        var count = System.Text.Encoding.UTF8.GetBytes(wkt, bytes);
        await Assert.That(WellKnownTextNode.Parse(bytes[..count])).IsNotDefault();
    }

    [Test]
    [Arguments("""
               PROJCRS["GDA2020 / Vicgrid",
               BASEGEOGCRS["GDA2020",
                   DATUM["Geocentric Datum of Australia 2020",
                       ELLIPSOID["GRS 1980",6378137,298.257222101,
                           LENGTHUNIT["metre",1]]],
                   PRIMEM["Greenwich",0,
                       ANGLEUNIT["degree",0.0174532925199433]],
                   ID["EPSG",7844]],
               CONVERSION["Vicgrid",
                   METHOD["Lambert Conic Conformal (2SP)",
                       ID["EPSG",9802]],
                   PARAMETER["Latitude of false origin",-37,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8821]],
                   PARAMETER["Longitude of false origin",145,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8822]],
                   PARAMETER["Latitude of 1st standard parallel",-36,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8823]],
                   PARAMETER["Latitude of 2nd standard parallel",-38,
                       ANGLEUNIT["degree",0.0174532925199433],
                       ID["EPSG",8824]],
                   PARAMETER["Easting at false origin",2500000,
                       LENGTHUNIT["metre",1],
                       ID["EPSG",8826]],
                   PARAMETER["Northing at false origin",2500000,
                       LENGTHUNIT["metre",1],
                       ID["EPSG",8827]]],
               CS[Cartesian,2],
                   AXIS["(E)",east,
                       ORDER[1],
                       LENGTHUNIT["metre",1]],
                   AXIS["(N)",north,
                       ORDER[2],
                       LENGTHUNIT["metre",1]],
               USAGE[
                   SCOPE["State-wide spatial data management."],
                   AREA["Australia - Victoria."],
                   BBOX[-39.2,140.96,-33.98,150.04]],
               ID["EPSG",7899]]
               """)]
    [Arguments("COMPD_CS[\"NAD83 / Oregon GIC Lambert (ft) + NAVD88 height (ftUS)\",PROJCS[\"NAD83 / Oregon GIC Lambert (ft)\",GEOGCS[\"NAD83\",DATUM[\"North_American_Datum_1983\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],AUTHORITY[\"EPSG\",\"6269\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4269\"]],PROJECTION[\"Lambert_Conformal_Conic_2SP\"],PARAMETER[\"latitude_of_origin\",41.75],PARAMETER[\"central_meridian\",-120.5],PARAMETER[\"standard_parallel_1\",43],PARAMETER[\"standard_parallel_2\",45.5],PARAMETER[\"false_easting\",1312335.958],PARAMETER[\"false_northing\",0],UNIT[\"foot\",0.3048,AUTHORITY[\"EPSG\",\"9002\"]],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH],AUTHORITY[\"EPSG\",\"2992\"]],VERT_CS[\"NAVD88 height (ftUS)\",VERT_DATUM[\"North American Vertical Datum 1988\",2005,AUTHORITY[\"EPSG\",\"5103\"]],UNIT[\"US survey foot\",0.304800609601219,AUTHORITY[\"EPSG\",\"9003\"]],AXIS[\"Gravity-related height\",UP],AUTHORITY[\"EPSG\",\"6360\"]]]")]
    public async Task CheckByteLength(string wkt)
    {
        Span<byte> bytes = stackalloc byte[System.Text.Encoding.UTF8.GetByteCount(wkt)];
        var node = WellKnownTextNode.Parse(wkt);
        var byteCount = node.GetByteCount();
        var bytesWritten = node.CopyTo(bytes);

        await Assert.That(byteCount).IsEqualTo(bytesWritten);
    }
}
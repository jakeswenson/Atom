PF.Atom
----
# Installation
You can install atom/hadron using Chocolatey and the PF-Chocolatey repo using:

```
choco install atom -s http://artifactory.hq.practicefusion.com:8081/artifactory/api/nuget/PF-Chocolatey
```

or update to the latest version of atom using:

```
choco upgrade atom -s http://artifactory.hq.practicefusion.com:8081/artifactory/api/nuget/PF-Chocolatey
```

## What is ATOM?

A way to decouple your data domain from direct sql tables.

Given an "atom" file (just json) it can:

- Auto generate SQL table creation 
- Auto generate Insert/Update/BatchList/List sprocs 
- Auto generates Views represneting data projections
- Auto generates C# data and repo classes
- Optionally enforces strong types (more explained below)

It's fully compatible with redgate sql schema compare as well as SQL server syntax.  

### Table/SPROc generation constructs:

- Unique keys
- Alternate keys 
- Hidden primary keys
- Optional nullable
- Queryable flag (indicating to generate an index)
- Compound keys
- Foreign keys
- Auto generated temporal values (`CreatedOnDateTimeUtc`, `LastModifiedDateTimeUtc`)
- Auto generated `IsActive` flag
- Group member configuration via groups.  

### View generation constructs

Views can be used as projections. What does this mean? This means if you want a column from several tables, you can create a ".proj" atom file and atom will automatically figure out how to join all your tables together (and even fill in missing joins!) to give you the data you want. 

On top of that, you also get strong typed data access to your view. You can leverage the pf.contrib.dapper.extensions library to query the view using strong typed expression trees (inspired by the MongoDB driver).

### Strong types

Strong types is another contrib package that wraps primitives in structs allowing you to "strongly type" value types.  When would you want to do this?  Imagine you have 5 different GUID's in your system correspnding to keys on tables. A `UserGuid` is not the same as a `ProfileGuid` and you should NOT be able to pass that into any function or DB layer.  

Atom can auto generate all your strong types, as well as serialization mappings and registration. To you, the user, you just use them as if they were their own types.

### Sample 

A sample atom file looks like:

```
{
    "name": "Test",
    "additionalInfo": {
        "hideKey": true,
        "hasAltKey": true,
        "hasActive": true,
        "hasTemporal": true
    },
    "members": {
        "DocumentGuid": { },
        "EhrPatientPracticeGuid": {
            "queryable": true,
            "unique": true,
        },
        "CareOrgGuid": {
            "reference": {
                "name": "CareOrg"
            }
        },
        "CareOrgName": { },
        "JakeId": {
            // type: "long", is implied by Id suffix
            optional: true
        },
        "TestGuid": {
            "isAltKey": true
        }
    },
    "indexOn": {
        columns: ["EhrPatientPracticeGuid", "CareOrgGuid"],
        unique: true
    }
}
```

### Contributors

@devshorts
@jakeswenson

echo Generating database dump...;
sqlite3 ../ELBv2/Assets/StreamingAssets/db.s3db .dump > ../ELBv2/DBDump/db.s3db.dump
echo Generated database dump;
git add ../ELBv2/DBDump/db.s3db.dump

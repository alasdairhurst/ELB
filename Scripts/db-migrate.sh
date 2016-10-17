echo "Backing up existing database";
cp ELBv2/Assets/StreamingAssets/db.s3db ELBv2/Assets/StreamingAssets/db.bak.s3db
echo "Importing database";
rm ELBv2/Assets/StreamingAssets/db.s3db
sqlite3 ELBv2/Assets/StreamingAssets/db.s3db < ELBv2/DBDump/db.s3db.dump
echo "[Imported database";
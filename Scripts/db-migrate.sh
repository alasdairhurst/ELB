if [ -f ../ELBv2/Assets/StreamingAssets/db.s3db ]; then
	echo "Backing up existing database";
	mv ../ELBv2/Assets/StreamingAssets/db.s3db ../ELBv2/Assets/StreamingAssets/bak/db.s3db.`date +"%s.%3N"`
fi
echo "Importing database";
sqlite3 ../ELBv2/Assets/StreamingAssets/db.s3db < ../ELBv2/DBDump/db.s3db.dump
echo "[Imported database]";

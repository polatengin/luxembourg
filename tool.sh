function pack() {
  echo "packing..."
  pushd "./src"
  dotnet build
  dotnet pack
  popd
}

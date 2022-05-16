function pack() {
  echo "packing..."
  pushd "./src"
  dotnet build
  dotnet pack
  popd
}

function install() {
  echo "installing..."
  pushd "./src"
  dotnet build
  dotnet pack
  dotnet tool install --global --add-source "./nupkg" "dcu"
  popd
}

function uninstall() {
  echo "uninstall"
}

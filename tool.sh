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

if [ "$ACTION" = "install" ]; then
  install
elif [ "$ACTION" = "uninstall" ]; then
  uninstall
elif [ "$ACTION" = "pack" ]; then
  pack
else
  echo "Usage: $0 [install|uninstall|pack]"
fi

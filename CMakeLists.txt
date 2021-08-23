cmake_minimum_required(VERSION 3.10)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE "Debug" CACHE STRING "" FORCE)
endif()

# This makes macOS find dylibs when we run?
set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
if(APPLE)
  set(CMAKE_INSTALL_RPATH "@executable_path;@loader_path")
else()
  set(CMAKE_INSTALL_RPATH "$ORIGIN")
endif()

project(ChessEngine C)

add_subdirectory(ChessEngine)

if(CMAKE_BUILD_TYPE MATCHES "Rel")
  set(DOTNET_BUILD_TYPE Release)
else()
  set(DOTNET_BUILD_TYPE Debug)
endif()

message("Dotnet build type: " ${DOTNET_BUILD_TYPE})

add_custom_command(
  OUTPUT ChessGUI
  COMMAND ${CMAKE_COMMAND} -E copy_directory
  ${CMAKE_CURRENT_LIST_DIR}/Chess/Assets ${CMAKE_BINARY_DIR}/Assets
  COMMAND dotnet build ${CMAKE_CURRENT_LIST_DIR} -c ${DOTNET_BUILD_TYPE} -o ${CMAKE_BINARY_DIR}
  )
add_custom_target(gui ALL
  DEPENDS ChessGUI
  )

set(ChessPublishDepends ChessEngine ChessEngineMain)

if(WIN32)
  list(APPEND ChessPublishDepends ChessCopyPthreads.dll ChessBundleCdlls)
  add_custom_command(
    OUTPUT ChessCopyPthreads.dll
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/pthread.dll ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy
            ${CMAKE_CURRENT_LIST_DIR}/ChessEngine/extern/librgl/extern/pthreads-win32-CMake/src/COPYING
            ${CMAKE_BINARY_DIR}/publish/pthreads-win32-COPYING
    COMMAND ${CMAKE_COMMAND} -E copy
            ${CMAKE_CURRENT_LIST_DIR}/ChessEngine/extern/librgl/extern/pthreads-win32-CMake/src/COPYING.lib
            ${CMAKE_BINARY_DIR}/publish/pthreads-win32-COPYING.lib
    )
  add_custom_command(
    OUTPUT ChessBundleCdlls
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_BINARY_DIR}/publish/
    # NOTE: This requires VC++ 2015 installed system wide (for developers)
    COMMAND ${CMAKE_COMMAND} -E copy /Windows/System32/ucrtbase.dll ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy /Windows/System32/vcruntime140.dll ${CMAKE_BINARY_DIR}/publish/
    )
endif()

if(WIN32)
  set(ExeExt .exe)
  set(DynamicLibExt dll)
  set(DotnetRID win-x64)
elseif(APPLE)
  set(DynamicLibExt dylib)
  set(DynamicLibPre lib)
  set(DotnetRID osx-x64)
elseif(UNIX)
  set(DynamicLibExt so)
  set(DynamicLibPre lib)
  set(DotnetRID linux-x64)
endif()

add_custom_command(
  OUTPUT ChessDotnetPublish
  COMMAND dotnet publish ${CMAKE_CURRENT_LIST_DIR}/Chess/Chess.csproj -o
  ${CMAKE_BINARY_DIR}/publish -c Release -r ${DotnetRID}
  COMMAND ${CMAKE_COMMAND} -E copy_directory
  ${CMAKE_CURRENT_LIST_DIR}/Chess/Assets ${CMAKE_BINARY_DIR}/publish/Assets
  )

add_custom_command(
  OUTPUT ChessPublish
  # TODO: Would be nice if there was a way to resolve a target's dependencies
  # automatically
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/ChessEngineMain${ExeExt} ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy
  ${CMAKE_BINARY_DIR}/${DynamicLibPre}ChessEngine.${DynamicLibExt} ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/${DynamicLibPre}rgl.${DynamicLibExt} ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/${DynamicLibPre}ipc.${DynamicLibExt} ${CMAKE_BINARY_DIR}/publish
  DEPENDS ${ChessPublishDepends} ChessDotnetPublish
  )

if(APPLE)
add_custom_command(
  OUTPUT Chess.app
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
  COMMAND ${CMAKE_COMMAND} -E rm -rf publish/Chess.app
  COMMAND ${CMAKE_COMMAND} -E make_directory Chess.app/Contents/Resources
  COMMAND ${CMAKE_COMMAND} -E make_directory Chess.app/Contents/CodeSignature
  COMMAND ${CMAKE_COMMAND} -E rename publish Chess.app/Contents/MacOS
  COMMAND ${CMAKE_COMMAND} -E rename Chess.app/Contents/MacOS/Assets/piece_black_castle.icns Chess.app/Contents/Resources/Chess.icns
  COMMAND ${CMAKE_COMMAND} -E make_directory publish
  COMMAND ${CMAKE_COMMAND} -E rename Chess.app publish/Chess.app
  COMMAND ${CMAKE_COMMAND} -E touch publish/Chess.app/Contents/Info.plist
  COMMAND echo "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\\n\
<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\\n\
<plist version=\"1.0\">\\n\
<dict>\\n\
    <key>CFBundleIconFile</key>\\n\
    <string>Chess.icns</string>\\n\
    <key>CFBundleName</key>\\n\
    <string>Chess</string>\\n\
    <key>CFBundleVersion</key>\\n\
    <string>0.1.0</string>\\n\
    <key>LSMinimumSystemVersion</key>\\n\
    <string>10.12</string>\\n\
    <key>CFBundleExecutable</key>\\n\
    <string>Chess</string>\\n\
    <key>CFBundleInfoDictionaryVersion</key>\\n\
    <string>6.0</string>\\n\
    <key>CFBundlePackageType</key>\\n\
    <string>APPL</string>\\n\
    <key>CFBundleShortVersionString</key>\\n\
    <string>1.0</string>\\n\
    <key>NSHighResolutionCapable</key>\\n\
    <true/>\\n\
</dict>\\n\
</plist>" > publish/Chess.app/Contents/Info.plist
  VERBATIM
  DEPENDS ChessPublish
  )
endif()

set(publishDepends ChessPublish)
if(APPLE)
  list(APPEND publishDepends Chess.app)
endif()
add_custom_target(publish DEPENDS ${publishDepends})

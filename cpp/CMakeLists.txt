# CMake minimum required version

cmake_minimum_required(VERSION 3.0)

# Solution name

project(CodePuzzles)

# Module's search pathes

set(CMAKE_MODULE_PATH
  ${PROJECT_SOURCE_DIR}/scripts
)

# Find SDL2

set(SDL2_DIR ${CMAKE_CURRENT_SOURCE_DIR}/3rdparty/SDL2-2.0.7)
find_package(SDL2 REQUIRED)

# Useful scripts inclusion

include(SubDirList)
include(ConfigureFiles)

# Problems projects inclusion

add_subdirectory(puzzles)
add_subdirectory(validation)

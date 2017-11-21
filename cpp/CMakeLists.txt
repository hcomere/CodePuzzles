# CMake minimum required version

cmake_minimum_required(VERSION 3.0)

# Solution name

project(CodePuzzles)

# Module's search pathes

set(CMAKE_MODULE_PATH
  ${PROJECT_SOURCE_DIR}/scripts
)

# Useful scripts inclusion

include(SubDirList)
include(ConfigureFiles)

# Problems projects inclusion

add_subdirectory(puzzles)
add_subdirectory(validation)

# Executable name definition

set(EXECUTABLE_NAME CodingGame_community_TheFastest)

enable_language(CSharp)

# Executable creation

add_executable(
    ${EXECUTABLE_NAME}
    Solution.cs
)

# Include directories

include_directories(../../../Validation)

# Dependencies

add_dependencies(
    ${EXECUTABLE_NAME}
    Validation
)

target_link_libraries(
    ${EXECUTABLE_NAME}
    Validation
)

# Copy data

configure_files(
    ${CMAKE_CURRENT_SOURCE_DIR}/data
    ${CMAKE_CURRENT_BINARY_DIR}/data
)